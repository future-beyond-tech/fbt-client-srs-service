using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRS.Application.Interfaces;
using SRS.Infrastructure.Configuration;

namespace SRS.Infrastructure.Services;

public sealed class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryFileStorageService> _logger;

    public CloudinaryFileStorageService(
        IOptions<CloudinarySettings> settings,
        ILogger<CloudinaryFileStorageService> logger)
    {
        var cloudinarySettings = settings.Value;

        if (string.IsNullOrWhiteSpace(cloudinarySettings.CloudName) ||
            string.IsNullOrWhiteSpace(cloudinarySettings.ApiKey) ||
            string.IsNullOrWhiteSpace(cloudinarySettings.ApiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary configuration is missing. Set Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret in user-secrets.");
        }

        _cloudinary = new Cloudinary(new Account(
            cloudinarySettings.CloudName,
            cloudinarySettings.ApiKey,
            cloudinarySettings.ApiSecret))
        {
            Api = { Secure = true }
        };
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new ArgumentException("File name is invalid.", nameof(fileName));
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        _logger.LogInformation("Starting Cloudinary invoice upload for file {FileName}.", safeFileName);

        try
        {
            var uploadResult = await _cloudinary.UploadAsync(
                new RawUploadParams
                {
                    File = new FileDescription(safeFileName, fileStream),
                    Folder = "srs/invoices",
                    PublicId = $"invoice-{Path.GetFileNameWithoutExtension(safeFileName)}-{Guid.NewGuid():N}",
                    Overwrite = false
                },
                "raw",
                cancellationToken);

            if (uploadResult.Error is not null)
            {
                _logger.LogError(
                    "Cloudinary invoice upload failed for file {FileName}. Error: {ErrorMessage}",
                    safeFileName,
                    uploadResult.Error.Message);

                throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            var secureUrl = uploadResult.SecureUrl?.ToString();
            if (string.IsNullOrWhiteSpace(secureUrl))
            {
                throw new InvalidOperationException("Cloudinary upload failed: secure URL was not returned.");
            }

            _logger.LogInformation(
                "Cloudinary invoice upload succeeded for file {FileName}. URL: {SecureUrl}",
                safeFileName,
                secureUrl);

            return secureUrl;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Cloudinary upload failure for file {FileName}.", safeFileName);
            throw new InvalidOperationException($"Cloudinary upload failed: {ex.Message}", ex);
        }
    }
}
