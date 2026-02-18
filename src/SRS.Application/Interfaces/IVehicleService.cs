using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IVehicleService
{
    Task<VehicleResponseDto> CreateAsync(VehicleCreateDto dto);
    Task<List<VehicleResponseDto>> GetAllAsync();
    Task<List<VehicleResponseDto>> GetAvailableAsync();
}
