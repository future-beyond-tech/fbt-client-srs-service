using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class VehicleService(AppDbContext context) : IVehicleService
{
    public Task<List<VehicleResponseDto>> GetAllAsync()
    {
        return context.Vehicles
            .AsNoTracking()
            .Select(v => new VehicleResponseDto
            {
                Id = v.Id,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year,
                RegistrationNumber = v.RegistrationNumber,
                ChassisNumber = v.ChassisNumber,
                EngineNumber = v.EngineNumber,
                Colour = v.Colour,
                SellingPrice = v.SellingPrice,
                Status = v.Status,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
    }

    public Task<List<VehicleResponseDto>> GetAvailableAsync()
    {
        return context.Vehicles
            .AsNoTracking()
            .Where(v => v.Status == VehicleStatus.Available)
            .Select(v => new VehicleResponseDto
            {
                Id = v.Id,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year,
                RegistrationNumber = v.RegistrationNumber,
                ChassisNumber = v.ChassisNumber,
                EngineNumber = v.EngineNumber,
                Colour = v.Colour,
                SellingPrice = v.SellingPrice,
                Status = v.Status,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync();
    }
}
