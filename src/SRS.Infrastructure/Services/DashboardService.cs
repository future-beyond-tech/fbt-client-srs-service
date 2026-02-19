using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class DashboardService(AppDbContext context) : IDashboardService
{
    public Task<DashboardDto> GetAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var startOfMonth = new DateTimeOffset(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero);



        return context.Database
            .SqlQuery<DashboardDto>(
                $"""
                 SELECT
                     (SELECT COUNT(1) FROM "Vehicles") AS "TotalVehiclesPurchased",
                     (SELECT COUNT(1) FROM "Vehicles" WHERE "Status" = {(int)VehicleStatus.Sold}) AS "TotalVehiclesSold",
                     (SELECT COUNT(1) FROM "Vehicles" WHERE "Status" = {(int)VehicleStatus.Available}) AS "AvailableVehicles",
                     (SELECT COALESCE(SUM("Profit"), 0) FROM "Sales") AS "TotalProfit",
                     (SELECT COALESCE(SUM(COALESCE("CashAmount", 0) + COALESCE("UpiAmount", 0) + COALESCE("FinanceAmount", 0)), 0)
                        FROM "Sales"
                       WHERE "SaleDate" >= {startOfMonth}) AS "SalesThisMonth"
                 """)
            .SingleAsync();
    }
}
