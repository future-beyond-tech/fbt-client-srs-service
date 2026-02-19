using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using SRS.Application.Interfaces;

namespace SRS.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new InvalidOperationException("Cloudinary configuration is missing.");
        }

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret))
        {
            Api = { Secure = true }
        };
    }

    public async Task<string> UploadPdfAsync(byte[] pdfBytes, string fileName, CancellationToken cancellationToken = default)
    {
        if (pdfBytes.Length == 0)
        {
            throw new ArgumentException("PDF payload cannot be empty.");
        }

        await using var stream = new MemoryStream(pdfBytes);
        var uploadResult = await _cloudinary.UploadAsync(
            new RawUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = $"invoices/{Path.GetFileNameWithoutExtension(fileName)}-{Guid.NewGuid():N}",
                Overwrite = false
            },
            "raw",
            cancellationToken);

        if (uploadResult.Error is not null)
        {
            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");
        }

        var secureUrl = uploadResult.SecureUrl?.ToString();
        if (string.IsNullOrWhiteSpace(secureUrl))
        {
            throw new InvalidOperationException("Cloudinary upload did not return a secure URL.");
        }

        return secureUrl;
    }
}
