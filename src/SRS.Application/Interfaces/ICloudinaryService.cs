using Microsoft.AspNetCore.Http;

namespace SRS.Application.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);
}
