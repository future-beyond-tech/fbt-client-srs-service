using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using SRS.Domain.Interfaces;

namespace SRS.Infrastructure.Services;

/// <summary>
/// Cloudinary implementation of <see cref="ICloudStorageService"/> for uploading delivery note PDFs.
/// Uses folder "invoices" and resource type "raw". Returns secure URL.
/// </summary>
public sealed class CloudinaryStorageService : ICloudStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(
        Cloudinary cloudinary,
        ILogger<CloudinaryStorageService> logger)
    {
        _cloudinary = cloudinary ?? throw new ArgumentNullException(nameof(cloudinary));
        _logger = logger;
    }

    public async Task<string> UploadPdfAsync(byte[] fileBytes, string fileName, CancellationToken ct = default)
    {
        if (fileBytes is null || fileBytes.Length == 0)
        {
            throw new ArgumentException("PDF content cannot be null or empty.", nameof(fileBytes));
        }

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        ct.ThrowIfCancellationRequested();

        await using var stream = new MemoryStream(fileBytes, writable: false);

        var publicId = Path.GetFileNameWithoutExtension(safeFileName);

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(safeFileName, stream),
            Folder = "invoices",
            PublicId = publicId,
            Overwrite = true,
        };

        _logger.LogInformation("Uploading delivery note PDF to Cloudinary: {FileName}.", safeFileName);

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
        {
            _logger.LogError(
                "Cloudinary PDF upload failed for {FileName}. Error: {ErrorMessage}",
                safeFileName,
                result.Error.Message);
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
        }

        var secureUrl = result.SecureUrl?.ToString();
        if (string.IsNullOrWhiteSpace(secureUrl))
        {
            throw new InvalidOperationException("Cloudinary upload failed: secure URL was not returned.");
        }

        _logger.LogInformation(
            "Cloudinary PDF upload succeeded for {FileName}. URL: {SecureUrl}",
            safeFileName,
            secureUrl);

        return secureUrl;
    }
}
