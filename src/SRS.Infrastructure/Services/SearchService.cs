using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class SearchService(AppDbContext context) : ISearchService
{
    public Task<List<SearchResultDto>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Task.FromResult(new List<SearchResultDto>());
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

        return context.Sales
            .AsNoTracking()
            .Where(s =>
                (hasBillNumber && s.BillNumber == parsedBillNumber) ||
                EF.Functions.ILike(s.Customer.Name, likePattern) ||
                EF.Functions.ILike(s.Customer.Phone, likePattern) ||
                EF.Functions.ILike(s.Vehicle.Brand, likePattern) ||
                EF.Functions.ILike(s.Vehicle.Model, likePattern) ||
                EF.Functions.ILike(s.Vehicle.RegistrationNumber, likePattern) ||
                (dayStart.HasValue && dayEnd.HasValue && s.SaleDate >= dayStart.Value && s.SaleDate < dayEnd.Value) ||
                (year.HasValue && s.SaleDate.Year == year.Value))
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new SearchResultDto
            {
                BillNumber = s.BillNumber,
                CustomerName = s.Customer.Name,
                CustomerPhone = s.Customer.Phone,
                Vehicle = s.Vehicle.Brand + " " + s.Vehicle.Model,
                RegistrationNumber = s.Vehicle.RegistrationNumber,
                SaleDate = s.SaleDate
            })
            .Take(50)
            .ToListAsync();
    }
}
