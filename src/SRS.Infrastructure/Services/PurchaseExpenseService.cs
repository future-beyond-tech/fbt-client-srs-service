using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class PurchaseExpenseService(AppDbContext context) : IPurchaseExpenseService
{
    public async Task<PurchaseExpenseDto> CreateAsync(int vehicleId, PurchaseExpenseCreateDto dto)
    {
        var vehicle = await context.Vehicles
            .Include(v => v.Purchase)
            .FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.Purchase is null)
        {
            throw new InvalidOperationException("Purchase record not found for the vehicle.");
        }

        if (vehicle.Status == VehicleStatus.Sold)
        {
            throw new InvalidOperationException("Cannot add expense for sold vehicle.");
        }

        var expense = new PurchaseExpense
        {
            VehicleId = vehicleId,
            ExpenseType = dto.ExpenseType.Trim(),
            Amount = dto.Amount,
            CreatedAt = DateTime.UtcNow
        };

        context.PurchaseExpenses.Add(expense);
        vehicle.Purchase.Expense += dto.Amount;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Map(expense);
    }

    public async Task<List<PurchaseExpenseDto>> GetByVehicleIdAsync(int vehicleId)
    {
        var vehicleExists = await context.Vehicles
            .AnyAsync(v => v.Id == vehicleId && !v.IsDeleted);

        if (!vehicleExists)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        return await context.PurchaseExpenses
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PurchaseExpenseDto
            {
                Id = x.Id,
                VehicleId = x.VehicleId,
                ExpenseType = x.ExpenseType,
                Amount = x.Amount,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task DeleteAsync(int expenseId)
    {
        var expense = await context.PurchaseExpenses
            .Include(x => x.Vehicle)
            .ThenInclude(v => v.Purchase)
            .FirstOrDefaultAsync(x => x.Id == expenseId);

        if (expense is null)
        {
            throw new KeyNotFoundException("Purchase expense not found.");
        }

        if (expense.Vehicle.IsDeleted)
        {
            throw new InvalidOperationException("Cannot modify expense for deleted vehicle.");
        }

        if (expense.Vehicle.Purchase is null)
        {
            throw new InvalidOperationException("Purchase record not found for the vehicle.");
        }

        if (expense.Vehicle.Status == VehicleStatus.Sold)
        {
            throw new InvalidOperationException("Cannot modify expense for sold vehicle.");
        }

        expense.Vehicle.Purchase.Expense = Math.Max(0, expense.Vehicle.Purchase.Expense - expense.Amount);
        expense.Vehicle.UpdatedAt = DateTime.UtcNow;

        context.PurchaseExpenses.Remove(expense);
        await context.SaveChangesAsync();
    }

    private static PurchaseExpenseDto Map(PurchaseExpense expense)
    {
        return new PurchaseExpenseDto
        {
            Id = expense.Id,
            VehicleId = expense.VehicleId,
            ExpenseType = expense.ExpenseType,
            Amount = expense.Amount,
            CreatedAt = expense.CreatedAt
        };
    }
}
