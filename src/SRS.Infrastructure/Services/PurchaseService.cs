using System.Data;
using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class PurchaseService(AppDbContext context) : IPurchaseService
{
    public async Task<PurchaseResponseDto> CreateAsync(PurchaseCreateDto dto)
    {
        ValidateRequest(dto);
        var registrationNumber = dto.RegistrationNumber.Trim();

        await using var transaction =
            await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

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
            ChassisNumber = string.IsNullOrWhiteSpace(dto.ChassisNumber) ? null : dto.ChassisNumber.Trim(),
            EngineNumber = string.IsNullOrWhiteSpace(dto.EngineNumber) ? null : dto.EngineNumber.Trim(),
            Colour = string.IsNullOrWhiteSpace(dto.Colour) ? null : dto.Colour.Trim(),
            SellingPrice = dto.SellingPrice,
            Status = VehicleStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        var purchase = new Purchase
        {
            Vehicle = vehicle,
            SellerName = dto.SellerName.Trim(),
            SellerPhone = dto.SellerPhone.Trim(),
            SellerAddress = string.IsNullOrWhiteSpace(dto.SellerAddress) ? null : dto.SellerAddress.Trim(),
            BuyingCost = dto.BuyingCost,
            Expense = dto.Expense,
            PurchaseDate = dto.PurchaseDate,
            CreatedAt = DateTime.UtcNow
        };

        context.Purchases.Add(purchase);

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException?.Message.Contains("RegistrationNumber", StringComparison.OrdinalIgnoreCase) == true)
        {
            throw new InvalidOperationException(
                $"A vehicle with registration number '{registrationNumber}' already exists.", ex);
        }

        return MapToDto(purchase);
    }

    public Task<List<PurchaseResponseDto>> GetAllAsync()
    {
        return context.Purchases
            .AsNoTracking()
            .Include(p => p.Vehicle)
            .OrderByDescending(p => p.PurchaseDate)
            .Select(p => new PurchaseResponseDto
            {
                Id = p.Id,
                VehicleId = p.VehicleId,
                Brand = p.Vehicle.Brand,
                Model = p.Vehicle.Model,
                Year = p.Vehicle.Year,
                RegistrationNumber = p.Vehicle.RegistrationNumber,
                Colour = p.Vehicle.Colour,
                SellingPrice = p.Vehicle.SellingPrice,
                SellerName = p.SellerName,
                SellerPhone = p.SellerPhone,
                SellerAddress = p.SellerAddress,
                BuyingCost = p.BuyingCost,
                Expense = p.Expense,
                PurchaseDate = p.PurchaseDate,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public Task<PurchaseResponseDto?> GetByIdAsync(int id)
    {
        return context.Purchases
            .AsNoTracking()
            .Include(p => p.Vehicle)
            .Where(p => p.Id == id)
            .Select(p => new PurchaseResponseDto
            {
                Id = p.Id,
                VehicleId = p.VehicleId,
                Brand = p.Vehicle.Brand,
                Model = p.Vehicle.Model,
                Year = p.Vehicle.Year,
                RegistrationNumber = p.Vehicle.RegistrationNumber,
                Colour = p.Vehicle.Colour,
                SellingPrice = p.Vehicle.SellingPrice,
                SellerName = p.SellerName,
                SellerPhone = p.SellerPhone,
                SellerAddress = p.SellerAddress,
                BuyingCost = p.BuyingCost,
                Expense = p.Expense,
                PurchaseDate = p.PurchaseDate,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    private static PurchaseResponseDto MapToDto(Purchase purchase)
    {
        return new PurchaseResponseDto
        {
            Id = purchase.Id,
            VehicleId = purchase.VehicleId,
            Brand = purchase.Vehicle.Brand,
            Model = purchase.Vehicle.Model,
            Year = purchase.Vehicle.Year,
            RegistrationNumber = purchase.Vehicle.RegistrationNumber,
            Colour = purchase.Vehicle.Colour,
            SellingPrice = purchase.Vehicle.SellingPrice,
            SellerName = purchase.SellerName,
            SellerPhone = purchase.SellerPhone,
            SellerAddress = purchase.SellerAddress,
            BuyingCost = purchase.BuyingCost,
            Expense = purchase.Expense,
            PurchaseDate = purchase.PurchaseDate,
            CreatedAt = purchase.CreatedAt
        };
    }

    private static void ValidateRequest(PurchaseCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Brand) ||
            string.IsNullOrWhiteSpace(dto.Model) ||
            string.IsNullOrWhiteSpace(dto.RegistrationNumber))
        {
            throw new ArgumentException("Brand, Model, and RegistrationNumber are required.");
        }

        if (string.IsNullOrWhiteSpace(dto.SellerName) ||
            string.IsNullOrWhiteSpace(dto.SellerPhone))
        {
            throw new ArgumentException("SellerName and SellerPhone are required.");
        }

        if (dto.PurchaseDate == default)
        {
            throw new ArgumentException("PurchaseDate is required.");
        }

        if (dto.BuyingCost < 0 || dto.Expense < 0 || dto.SellingPrice < 0)
        {
            throw new ArgumentException("BuyingCost, Expense, and SellingPrice cannot be negative.");
        }
    }
}
