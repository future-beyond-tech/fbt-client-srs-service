using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class VehicleService(AppDbContext context) : IVehicleService
{
    public async Task<VehicleResponseDto> CreateAsync(VehicleCreateDto dto)
    {
        var registrationNumber = dto.RegistrationNumber.Trim();

        var exists = await context.Vehicles
            .AnyAsync(v => v.RegistrationNumber == registrationNumber);

        if (exists)
        {
            throw new InvalidOperationException(
                $"A vehicle with registration number '{registrationNumber}' already exists.");
        }

        var vehicle = new Vehicle
        {
            Brand = dto.Brand.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            RegistrationNumber = registrationNumber,
            ChassisNumber = dto.ChassisNumber.Trim(),
            EngineNumber = dto.EngineNumber.Trim(),
            BuyingCost = dto.BuyingCost,
            Expense = dto.Expense,
            SellingPrice = dto.SellingPrice,
            PurchaseDate = dto.PurchaseDate,
            Status = VehicleStatus.Available
        };

        context.Vehicles.Add(vehicle);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("RegistrationNumber", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException(
                $"A vehicle with registration number '{registrationNumber}' already exists.", ex);
        }

        return new VehicleResponseDto
        {
            Id = vehicle.Id,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            RegistrationNumber = vehicle.RegistrationNumber,
            ChassisNumber = vehicle.ChassisNumber,
            EngineNumber = vehicle.EngineNumber,
            SellingPrice = vehicle.SellingPrice,
            Status = vehicle.Status,
            PurchaseDate = vehicle.PurchaseDate
        };
    }

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
                SellingPrice = v.SellingPrice,
                Status = v.Status,
                PurchaseDate = v.PurchaseDate
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
                SellingPrice = v.SellingPrice,
                Status = v.Status,
                PurchaseDate = v.PurchaseDate
            })
            .ToListAsync();
    }
}
