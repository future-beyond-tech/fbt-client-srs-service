using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRS.Domain.Interfaces;
using SRS.Infrastructure.Configuration;

namespace SRS.Infrastructure.Services;

/// <summary>
/// Cloudinary implementation of <see cref="ICloudStorageService"/> for uploading delivery note PDFs.
/// Uses folder from CloudinarySettings (or "invoices"). Returns HTTPS secure URL. Never logs API secret.
/// </summary>
public sealed class CloudinaryStorageService(
    Cloudinary cloudinary,
    IOptions<CloudinarySettings> settings,
    ILogger<CloudinaryStorageService> logger) : ICloudStorageService
{
    private readonly string _folder = string.IsNullOrWhiteSpace(settings.Value.Folder) ? "invoices" : settings.Value.Folder.Trim();

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
            Folder = _folder,
            PublicId = publicId,
            Overwrite = true,
        };

        logger.LogInformation("Uploading delivery note PDF to Cloudinary folder {Folder}: {FileName}.", _folder, safeFileName);

        var result = await cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
        {
            logger.LogError(
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

        if (!secureUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cloudinary upload failed: secure URL must be HTTPS.");
        }

        logger.LogInformation(
            "Cloudinary PDF upload succeeded for {FileName}. URL: {SecureUrl}",
            safeFileName,
            secureUrl);

        return secureUrl;
    }
}
