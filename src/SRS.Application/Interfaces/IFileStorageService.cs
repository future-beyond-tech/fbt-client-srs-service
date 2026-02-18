using Microsoft.AspNetCore.Http;

namespace SRS.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveCustomerPhotoAsync(IFormFile file);
}
