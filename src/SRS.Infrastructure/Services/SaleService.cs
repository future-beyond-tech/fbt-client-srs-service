using System.Data;
using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class SaleService(AppDbContext context) : ISaleService
{
    public async Task<SaleResponseDto> CreateAsync(SaleCreateDto dto)
    {
        ValidateRequest(dto);

        await using var transaction =
            await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var vehicle = await context.Vehicles
            .Include(v => v.Purchase)
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.Purchase is null)
        {
            throw new InvalidOperationException("Purchase record not found for the vehicle.");
        }

        if (vehicle.Status != VehicleStatus.Available)
        {
            throw new InvalidOperationException("Vehicle already sold.");
        }

        var hasExistingSale = await context.Sales
            .AnyAsync(s => s.VehicleId == dto.VehicleId);

        if (hasExistingSale)
        {
            throw new InvalidOperationException("Vehicle already sold.");
        }

        var billNumber = await GenerateBillNumberAsync();
        var cashAmount = dto.CashAmount ?? 0m;
        var upiAmount = dto.UpiAmount ?? 0m;
        var financeAmount = dto.FinanceAmount ?? 0m;
        var totalReceived = cashAmount + upiAmount + financeAmount;
        var profit = vehicle.SellingPrice - (vehicle.Purchase.BuyingCost + vehicle.Purchase.Expense);

        var sale = new Sale
        {
            BillNumber = billNumber,
            VehicleId = vehicle.Id,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            CustomerAddress = string.IsNullOrWhiteSpace(dto.CustomerAddress) ? null : dto.CustomerAddress.Trim(),
            CustomerPhotoUrl = dto.CustomerPhotoUrl.Trim(),
            PaymentMode = dto.PaymentMode,
            CashAmount = dto.CashAmount,
            UpiAmount = dto.UpiAmount,
            FinanceAmount = dto.FinanceAmount,
            FinanceCompany = string.IsNullOrWhiteSpace(dto.FinanceCompany) ? null : dto.FinanceCompany.Trim(),
            SaleDate = dto.SaleDate,
            Profit = profit
        };

        vehicle.Status = VehicleStatus.Sold;
        context.Sales.Add(sale);

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraint(ex, "BillNumber"))
        {
            throw new InvalidOperationException("Bill number collision occurred. Please retry.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraint(ex, "VehicleId"))
        {
            throw new InvalidOperationException("Vehicle already sold.", ex);
        }

        return new SaleResponseDto
        {
            BillNumber = sale.BillNumber,
            VehicleId = vehicle.Id,
            Vehicle = $"{vehicle.Brand} {vehicle.Model}",
            CustomerName = sale.CustomerName,
            TotalReceived = totalReceived,
            Profit = sale.Profit,
            SaleDate = sale.SaleDate
        };
    }

    public Task<BillDetailDto?> GetByBillNumberAsync(int billNumber)
    {
        return context.Sales
            .AsNoTracking()
            .Include(s => s.Vehicle)
            .ThenInclude(v => v.Purchase)
            .Where(s => s.BillNumber == billNumber)
            .Select(s => new BillDetailDto
            {
                BillNumber = s.BillNumber,
                SaleDate = s.SaleDate,
                VehicleId = s.VehicleId,
                Brand = s.Vehicle.Brand,
                Model = s.Vehicle.Model,
                Year = s.Vehicle.Year,
                RegistrationNumber = s.Vehicle.RegistrationNumber,
                ChassisNumber = s.Vehicle.ChassisNumber,
                EngineNumber = s.Vehicle.EngineNumber,
                SellingPrice = s.Vehicle.SellingPrice,
                CustomerName = s.CustomerName,
                CustomerPhone = s.CustomerPhone,
                CustomerAddress = s.CustomerAddress,
                PurchaseDate = s.Vehicle.Purchase.PurchaseDate,
                BuyingCost = s.Vehicle.Purchase.BuyingCost,
                Expense = s.Vehicle.Purchase.Expense,
                PaymentMode = s.PaymentMode,
                CashAmount = s.CashAmount,
                UpiAmount = s.UpiAmount,
                FinanceAmount = s.FinanceAmount,
                FinanceCompany = s.FinanceCompany,
                Profit = s.Profit,
                TotalReceived = (s.CashAmount ?? 0m) + (s.UpiAmount ?? 0m) + (s.FinanceAmount ?? 0m)
            })
            .FirstOrDefaultAsync();
    }

    private async Task<int> GenerateBillNumberAsync()
    {
        var maxBillNumber = await context.Sales
            .MaxAsync(s => (int?)s.BillNumber);

        return (maxBillNumber ?? 0) + 1;
    }

    private static void ValidateRequest(SaleCreateDto dto)
    {
        if (dto.VehicleId <= 0)
        {
            throw new ArgumentException("VehicleId is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.CustomerName) ||
            string.IsNullOrWhiteSpace(dto.CustomerPhone))
        {
            throw new ArgumentException("CustomerName and CustomerPhone are required.");
        }

        if (string.IsNullOrWhiteSpace(dto.CustomerPhotoUrl))
        {
            throw new ArgumentException("CustomerPhotoUrl is required.");
        }

        if (!Enum.IsDefined(dto.PaymentMode))
        {
            throw new ArgumentException("Invalid payment mode.");
        }

        if (dto.SaleDate == default)
        {
            throw new ArgumentException("SaleDate is required.");
        }

        var cashAmount = dto.CashAmount ?? 0m;
        var upiAmount = dto.UpiAmount ?? 0m;
        var financeAmount = dto.FinanceAmount ?? 0m;

        if (cashAmount < 0 || upiAmount < 0 || financeAmount < 0)
        {
            throw new ArgumentException("Payment amounts cannot be negative.");
        }

        var totalReceived = cashAmount + upiAmount + financeAmount;
        if (totalReceived <= 0)
        {
            throw new ArgumentException("At least one payment amount must be greater than zero.");
        }

        switch (dto.PaymentMode)
        {
            case PaymentMode.Cash when cashAmount <= 0 || upiAmount != 0 || financeAmount != 0:
                throw new ArgumentException("Cash mode requires only CashAmount.");
            case PaymentMode.UPI when upiAmount <= 0 || cashAmount != 0 || financeAmount != 0:
                throw new ArgumentException("Upi mode requires only UpiAmount.");
            case PaymentMode.Finance when financeAmount <= 0 || cashAmount != 0 || upiAmount != 0:
                throw new ArgumentException("Finance mode requires only FinanceAmount.");
        }

        if (financeAmount > 0 && string.IsNullOrWhiteSpace(dto.FinanceCompany))
        {
            throw new ArgumentException("FinanceCompany is required when FinanceAmount is used.");
        }
    }

    private static bool IsUniqueConstraint(DbUpdateException ex, string keyName)
    {
        return ex.InnerException?.Message.Contains(keyName, StringComparison.OrdinalIgnoreCase) == true;
    }
}
