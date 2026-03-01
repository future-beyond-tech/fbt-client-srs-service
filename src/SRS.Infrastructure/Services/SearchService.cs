using Microsoft.EntityFrameworkCore;
using SRS.Application.Common;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class SearchService(AppDbContext context) : ISearchService
{
    private const int MaxSearchResults = 50;

    public async Task<List<SearchResultDto>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<SearchResultDto>();
        }

        var normalizedKeyword = keyword.Trim();
        var likePattern = $"%{normalizedKeyword}%";
        var hasBillNumber = int.TryParse(normalizedKeyword, out var parsedBillNumber);
        var dayStart = DateTime.TryParse(normalizedKeyword, out var date)
            ? date.Date
            : (DateTime?)null;
        var dayEnd = dayStart?.AddDays(1);
        var year = normalizedKeyword.Length == 4 && int.TryParse(normalizedKeyword, out var parsedYear)
            ? parsedYear
            : (int?)null;

        var salesQuery = context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Vehicle)
            .Where(s => s.Customer != null && s.Vehicle != null)
            .Where(s =>
                (hasBillNumber && s.BillNumber == parsedBillNumber) ||
                EF.Functions.ILike(s.Customer!.Name, likePattern) ||
                EF.Functions.ILike(s.Customer.Phone, likePattern) ||
                EF.Functions.ILike(s.Vehicle!.Brand, likePattern) ||
                EF.Functions.ILike(s.Vehicle.Model, likePattern) ||
                EF.Functions.ILike(s.Vehicle.RegistrationNumber, likePattern) ||
                (dayStart.HasValue && dayEnd.HasValue && s.SaleDate >= dayStart.Value && s.SaleDate < dayEnd.Value) ||
                (year.HasValue && s.SaleDate.Year == year.Value))
            .OrderByDescending(s => s.SaleDate)
            .Take(MaxSearchResults);

        var manualBillsQuery = context.ManualBills
            .AsNoTracking()
            .Where(b =>
                (hasBillNumber && b.BillNumber == parsedBillNumber) ||
                EF.Functions.ILike(b.CustomerName, likePattern) ||
                EF.Functions.ILike(b.Phone, likePattern) ||
                EF.Functions.ILike(b.ItemDescription, likePattern) ||
                (dayStart.HasValue && dayEnd.HasValue && b.CreatedAtUtc >= dayStart.Value && b.CreatedAtUtc < dayEnd.Value) ||
                (year.HasValue && b.CreatedAtUtc.Year == year.Value))
            .OrderByDescending(b => b.CreatedAtUtc)
            .Take(MaxSearchResults);

        var sales = await salesQuery
            .Select(s => new SearchResultDto
            {
                Type = "Sale",
                BillNumber = s.BillNumber,
                CustomerName = s.Customer!.Name,
                CustomerPhone = s.Customer.Phone,
                Vehicle = s.Vehicle!.Brand + " " + s.Vehicle.Model,
                RegistrationNumber = s.Vehicle.RegistrationNumber,
                SaleDate = s.SaleDate
            })
            .ToListAsync();

        var manualBills = await manualBillsQuery
            .Select(b => new SearchResultDto
            {
                Type = "ManualBill",
                BillNumber = b.BillNumber,
                CustomerName = b.CustomerName,
                CustomerPhone = b.Phone,
                Vehicle = (string?)null,
                RegistrationNumber = (string?)null,
                SaleDate = b.CreatedAtUtc
            })
            .ToListAsync();

        foreach (var dto in sales)
            dto.CustomerPhone = PhoneMask.MaskLastFour(dto.CustomerPhone);
        foreach (var dto in manualBills)
            dto.CustomerPhone = PhoneMask.MaskLastFour(dto.CustomerPhone);

        var combined = sales
            .Concat(manualBills)
            .OrderByDescending(x => x.SaleDate)
            .Take(MaxSearchResults)
            .ToList();

        return combined;
    }
}
