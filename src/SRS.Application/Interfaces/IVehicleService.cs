using SRS.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace SRS.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleResponseDto>> GetAllAsync();
    Task<VehicleResponseDto?> GetByIdAsync(int id);
    Task<List<VehicleResponseDto>> GetAvailableAsync();
    Task<VehicleResponseDto> UpdateVehicleAsync(int id, VehicleUpdateDto dto);
    Task SoftDeleteVehicleAsync(int id);
    Task<VehicleResponseDto> UpdateVehicleStatusAsync(int id, VehicleStatusUpdateDto dto);
    Task<VehiclePhotoUploadResponseDto> UploadPhotosAsync(int vehicleId, IReadOnlyCollection<IFormFile> files);
    Task<List<VehiclePhotoDto>> SetPrimaryPhotoAsync(int vehicleId, int photoId);
    Task<List<VehiclePhotoDto>> DeletePhotoAsync(int photoId);
}
