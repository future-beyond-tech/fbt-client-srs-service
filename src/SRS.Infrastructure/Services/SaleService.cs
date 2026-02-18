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
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
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

        var phone = dto.Phone.Trim();
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone);

        if (customer is null)
        {
            customer = new Customer
            {
                Name = dto.CustomerName.Trim(),
                Phone = phone,
                Address = dto.Address.Trim(),
                PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl.Trim()
            };

            context.Customers.Add(customer);
        }

        var billNumber = await GenerateBillNumberAsync();
        var totalReceived = dto.CashAmount + dto.UpiAmount + dto.FinanceAmount;
        var profit = totalReceived - (vehicle.BuyingCost + vehicle.Expense);

        var sale = new Sale
        {
            BillNumber = billNumber,
            VehicleId = vehicle.Id,
            CustomerId = customer.Id,
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
            Vehicle = $"{vehicle.Brand} {vehicle.Model}",
            CustomerName = customer.Name,
            TotalReceived = totalReceived,
            Profit = sale.Profit,
            SaleDate = sale.SaleDate
        };
    }

    public Task<SaleResponseDto?> GetByBillNumberAsync(string billNumber)
    {
        var normalizedBillNumber = billNumber.Trim();

        return context.Sales
            .AsNoTracking()
            .Where(s => s.BillNumber == normalizedBillNumber)
            .Select(s => new SaleResponseDto
            {
                BillNumber = s.BillNumber,
                Vehicle = s.Vehicle.Brand + " " + s.Vehicle.Model,
                CustomerName = s.Customer.Name,
                TotalReceived = s.CashAmount + s.UpiAmount + s.FinanceAmount,
                Profit = s.Profit,
                SaleDate = s.SaleDate
            })
            .FirstOrDefaultAsync();
    }

    public Task<BillDetailDto?> GetBillDetailAsync(string billNumber)
    {
        var normalizedBillNumber = billNumber.Trim();

        return context.Sales
            .AsNoTracking()
            .Where(s => s.BillNumber == normalizedBillNumber)
            .Select(s => new BillDetailDto
            {
                BillNumber = s.BillNumber,
                SaleDate = s.SaleDate,
                Brand = s.Vehicle.Brand,
                Model = s.Vehicle.Model,
                Year = s.Vehicle.Year,
                RegistrationNumber = s.Vehicle.RegistrationNumber,
                ChassisNumber = s.Vehicle.ChassisNumber,
                EngineNumber = s.Vehicle.EngineNumber,
                CustomerName = s.Customer.Name,
                Phone = s.Customer.Phone,
                Address = s.Customer.Address,
                PhotoUrl = s.Customer.PhotoUrl,
                CashAmount = s.CashAmount,
                UpiAmount = s.UpiAmount,
                FinanceAmount = s.FinanceAmount,
                FinanceCompany = s.FinanceCompany,
                TotalReceived = s.CashAmount + s.UpiAmount + s.FinanceAmount
            })
            .FirstOrDefaultAsync();
    }

    private async Task<string> GenerateBillNumberAsync()
    {
        var year = DateTime.UtcNow.Year;

        var count = await context.Sales
            .CountAsync(s => s.SaleDate.Year == year);

        return $"SRS-{year}-{(count + 1):D4}";
    }

    private static void ValidateRequest(SaleCreateDto dto)
    {
        if (dto.VehicleId == Guid.Empty)
        {
            throw new ArgumentException("VehicleId is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.CustomerName) ||
            string.IsNullOrWhiteSpace(dto.Phone) ||
            string.IsNullOrWhiteSpace(dto.Address))
        {
            throw new ArgumentException("CustomerName, Phone, and Address are required.");
        }

        if (!Enum.IsDefined(dto.PaymentMode))
        {
            throw new ArgumentException("Invalid payment mode.");
        }

        if (dto.SaleDate == default)
        {
            throw new ArgumentException("SaleDate is required.");
        }

        if (dto.CashAmount < 0 || dto.UpiAmount < 0 || dto.FinanceAmount < 0)
        {
            throw new ArgumentException("Payment amounts cannot be negative.");
        }

        var totalReceived = dto.CashAmount + dto.UpiAmount + dto.FinanceAmount;
        if (totalReceived <= 0)
        {
            throw new ArgumentException("At least one payment amount must be greater than zero.");
        }

        switch (dto.PaymentMode)
        {
            case PaymentMode.Cash when dto.CashAmount <= 0 || dto.UpiAmount != 0 || dto.FinanceAmount != 0:
                throw new ArgumentException("Cash mode requires only CashAmount.");
            case PaymentMode.Upi when dto.UpiAmount <= 0 || dto.CashAmount != 0 || dto.FinanceAmount != 0:
                throw new ArgumentException("Upi mode requires only UpiAmount.");
            case PaymentMode.Finance when dto.FinanceAmount <= 0 || dto.CashAmount != 0 || dto.UpiAmount != 0:
                throw new ArgumentException("Finance mode requires only FinanceAmount.");
            case PaymentMode.Mixed when (dto.CashAmount > 0 ? 1 : 0) + (dto.UpiAmount > 0 ? 1 : 0) + (dto.FinanceAmount > 0 ? 1 : 0) < 2:
                throw new ArgumentException("Mixed mode requires at least two non-zero payment amounts.");
        }

        if (dto.FinanceAmount > 0 && string.IsNullOrWhiteSpace(dto.FinanceCompany))
        {
            throw new ArgumentException("FinanceCompany is required when FinanceAmount is used.");
        }
    }

    private static bool IsUniqueConstraint(DbUpdateException ex, string keyName)
    {
        return ex.InnerException?.Message.Contains(keyName, StringComparison.OrdinalIgnoreCase) == true;
    }
}
