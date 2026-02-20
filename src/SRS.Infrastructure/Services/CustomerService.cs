using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class CustomerService(AppDbContext context) : ICustomerService
{
    public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto)
    {
        ValidateCreateRequest(dto);

        var customer = new Customer
        {
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            PhotoUrl = null,
            CreatedAt = DateTime.UtcNow
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return Map(customer);
    }

    public Task<List<CustomerDto>> GetAllAsync()
    {
        return context.Customers
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                PhotoUrl = c.PhotoUrl,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        return context.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                PhotoUrl = c.PhotoUrl,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public Task<List<CustomerDto>> SearchAsync(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return Task.FromResult(new List<CustomerDto>());
        }

        var term = phone.Trim();
        var like = $"%{term}%";

        return context.Customers
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Phone, like) || EF.Functions.ILike(c.Name, like))
            .OrderByDescending(c => c.CreatedAt)
            .Take(20)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                PhotoUrl = c.PhotoUrl,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    private static CustomerDto Map(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            Address = customer.Address,
            PhotoUrl = customer.PhotoUrl,
            CreatedAt = customer.CreatedAt
        };
    }

    private static void ValidateCreateRequest(CustomerCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Phone))
        {
            throw new ArgumentException("Phone is required.");
        }
    }
}
