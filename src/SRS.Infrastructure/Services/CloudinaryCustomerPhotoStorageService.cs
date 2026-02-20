using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRS.Application.Interfaces;
using SRS.Infrastructure.Configuration;

namespace SRS.Infrastructure.Services;

public sealed class CloudinaryCustomerPhotoStorageService : ICustomerPhotoStorageService
{
    private const long MaxFileSize = 2 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryCustomerPhotoStorageService> _logger;

    public CloudinaryCustomerPhotoStorageService(
        IOptions<CloudinarySettings> settings,
        ILogger<CloudinaryCustomerPhotoStorageService> logger)
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

    public async Task<string> SaveCustomerPhotoAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Invalid file.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException("File size exceeds 2MB.");
        }

        if (!AllowedTypes.TryGetValue(file.ContentType, out var extension))
        {
            throw new ArgumentException("Only JPG, PNG, WEBP images are allowed.");
        }

        await using var input = file.OpenReadStream();
        var hasValidSignature = await HasValidSignatureAsync(input, file.ContentType);
        if (!hasValidSignature)
        {
            throw new ArgumentException("File content does not match a supported image format.");
        }

        if (input.CanSeek)
        {
            input.Position = 0;
        }

        var originalFileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            originalFileName = $"customer-{Guid.NewGuid():N}{extension}";
        }

        _logger.LogInformation("Starting Cloudinary customer photo upload for {FileName}.", originalFileName);

        var uploadResult = await _cloudinary.UploadAsync(
            new ImageUploadParams
            {
                File = new FileDescription(originalFileName, input),
                Folder = "srs/customers",
                PublicId = $"customer-{Guid.NewGuid():N}",
                Overwrite = false
            });

        if (uploadResult.Error is not null)
        {
            _logger.LogError(
                "Cloudinary customer photo upload failed for {FileName}. Error: {ErrorMessage}",
                originalFileName,
                uploadResult.Error.Message);

            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        var secureUrl = uploadResult.SecureUrl?.ToString();
        if (string.IsNullOrWhiteSpace(secureUrl))
        {
            throw new InvalidOperationException("Cloudinary upload failed: secure URL was not returned.");
        }

        _logger.LogInformation(
            "Cloudinary customer photo upload succeeded for {FileName}. URL: {SecureUrl}",
            originalFileName,
            secureUrl);

        return secureUrl;
    }

    private static async Task<bool> HasValidSignatureAsync(Stream input, string contentType)
    {
        var header = new byte[12];
        var read = await input.ReadAsync(header);

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => read >= 3 &&
                            header[0] == 0xFF &&
                            header[1] == 0xD8 &&
                            header[2] == 0xFF,
            "image/png" => read >= 8 &&
                           header[0] == 0x89 &&
                           header[1] == 0x50 &&
                           header[2] == 0x4E &&
                           header[3] == 0x47 &&
                           header[4] == 0x0D &&
                           header[5] == 0x0A &&
                           header[6] == 0x1A &&
                           header[7] == 0x0A,
            "image/webp" => read >= 12 &&
                            header[0] == 0x52 &&
                            header[1] == 0x49 &&
                            header[2] == 0x46 &&
                            header[3] == 0x46 &&
                            header[8] == 0x57 &&
                            header[9] == 0x45 &&
                            header[10] == 0x42 &&
                            header[11] == 0x50,
            _ => false
        };
    }
}
