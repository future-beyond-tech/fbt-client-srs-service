using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class FinanceCompanyService(AppDbContext context) : IFinanceCompanyService
{
    public async Task<FinanceCompanyDto> CreateAsync(FinanceCompanyCreateDto dto)
    {
        var normalizedName = dto.Name.Trim();
        var duplicateExists = await context.FinanceCompanies
            .AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower());

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"Finance company '{normalizedName}' already exists.");
        }

        var financeCompany = new FinanceCompany
        {
            Name = normalizedName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.FinanceCompanies.Add(financeCompany);
        await context.SaveChangesAsync();

        return Map(financeCompany);
    }

    public Task<List<FinanceCompanyDto>> GetAllAsync()
    {
        return context.FinanceCompanies
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new FinanceCompanyDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var financeCompany = await context.FinanceCompanies
            .FirstOrDefaultAsync(x => x.Id == id);

        if (financeCompany is null)
        {
            throw new KeyNotFoundException("Finance company not found.");
        }

        if (!financeCompany.IsActive)
        {
            throw new InvalidOperationException("Finance company is already inactive.");
        }

        financeCompany.IsActive = false;
        await context.SaveChangesAsync();
    }

    private static FinanceCompanyDto Map(FinanceCompany company)
    {
        return new FinanceCompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            IsActive = company.IsActive,
            CreatedAt = company.CreatedAt
        };
    }
}
