using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> CreateAsync(CustomerCreateDto dto);
    Task<List<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<List<CustomerDto>> SearchAsync(string phone);
}
