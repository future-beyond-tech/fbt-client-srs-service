using Microsoft.AspNetCore.Http;

namespace SRS.Application.Interfaces;

public interface ICustomerPhotoStorageService
{
    Task<string> SaveCustomerPhotoAsync(IFormFile file);
}
