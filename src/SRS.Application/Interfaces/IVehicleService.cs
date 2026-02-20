using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleResponseDto>> GetAllAsync();
    Task<List<VehicleResponseDto>> GetAvailableAsync();
    Task<VehicleResponseDto> UpdateVehicleAsync(int id, VehicleUpdateDto dto);
    Task SoftDeleteVehicleAsync(int id);
    Task<VehicleResponseDto> UpdateVehicleStatusAsync(int id, VehicleStatusUpdateDto dto);
}
