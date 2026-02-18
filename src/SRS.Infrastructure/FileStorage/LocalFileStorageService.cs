using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SRS.Application.Interfaces;

namespace SRS.Infrastructure.FileStorage;

public class LocalFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private const long MaxFileSize = 2 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

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

        var uploadsPath = Path.Combine(environment.ContentRootPath, "Uploads", "customers");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var output = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(output);

        return $"/uploads/customers/{fileName}";
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
