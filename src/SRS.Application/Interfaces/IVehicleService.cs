using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleResponseDto>> GetAllAsync();
    Task<List<VehicleResponseDto>> GetAvailableAsync();
}
