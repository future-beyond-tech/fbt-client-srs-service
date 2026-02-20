using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class VehicleService(
    AppDbContext context,
    ILogger<VehicleService> logger) : IVehicleService
{
    public Task<List<VehicleResponseDto>> GetAllAsync()
    {
        return context.Vehicles
            .AsNoTracking()
            .Where(v => !v.IsDeleted)
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
            .Where(v => !v.IsDeleted && v.Status == VehicleStatus.Available)
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

    public async Task<VehicleResponseDto> UpdateVehicleAsync(int id, VehicleUpdateDto dto)
    {
        var vehicle = await context.Vehicles
            .Include(v => v.Purchase)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null || vehicle.IsDeleted)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.Purchase is null)
        {
            throw new InvalidOperationException("Purchase record not found for the vehicle.");
        }

        if (vehicle.Status == VehicleStatus.Sold)
        {
            throw new InvalidOperationException("Sold vehicle cannot be updated.");
        }

        if (!string.IsNullOrWhiteSpace(dto.RegistrationNumber))
        {
            var registrationNumber = dto.RegistrationNumber.Trim();

            var duplicateExists = await context.Vehicles
                .AnyAsync(v => v.Id != id &&
                               !v.IsDeleted &&
                               v.RegistrationNumber == registrationNumber);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    $"A vehicle with registration number '{registrationNumber}' already exists.");
            }

            vehicle.RegistrationNumber = registrationNumber;
        }

        var totalAcquisitionCost = vehicle.Purchase.BuyingCost + vehicle.Purchase.Expense;
        if (dto.SellingPrice < totalAcquisitionCost)
        {
            throw new ArgumentException(
                $"SellingPrice cannot be less than total acquisition cost ({totalAcquisitionCost:0.00}).");
        }

        vehicle.SellingPrice = dto.SellingPrice;
        vehicle.Colour = string.IsNullOrWhiteSpace(dto.Colour) ? null : dto.Colour.Trim();
        vehicle.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return MapVehicle(vehicle);
    }

    public async Task SoftDeleteVehicleAsync(int id)
    {
        var vehicle = await context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.IsDeleted)
        {
            throw new InvalidOperationException("Vehicle is already deleted.");
        }

        if (vehicle.Status == VehicleStatus.Sold)
        {
            throw new InvalidOperationException("Sold vehicle cannot be deleted.");
        }

        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<VehicleResponseDto> UpdateVehicleStatusAsync(int id, VehicleStatusUpdateDto dto)
    {
        var vehicle = await context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle is null || vehicle.IsDeleted)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.Status == VehicleStatus.Sold && dto.Status == VehicleStatus.Available)
        {
            throw new InvalidOperationException("Cannot change status from Sold back to Available.");
        }

        var previousStatus = vehicle.Status;
        if (previousStatus != dto.Status)
        {
            vehicle.Status = dto.Status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            logger.LogInformation(
                "Vehicle status changed for VehicleId {VehicleId} from {PreviousStatus} to {NewStatus}.",
                vehicle.Id,
                previousStatus,
                dto.Status);

            await context.SaveChangesAsync();
        }

        return MapVehicle(vehicle);
    }

    private static VehicleResponseDto MapVehicle(Vehicle v)
    {
        return new VehicleResponseDto
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
        };
    }
}
