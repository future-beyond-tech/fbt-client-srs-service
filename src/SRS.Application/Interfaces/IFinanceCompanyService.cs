using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IFinanceCompanyService
{
    Task<FinanceCompanyDto> CreateAsync(FinanceCompanyCreateDto dto);
    Task<List<FinanceCompanyDto>> GetAllAsync();
    Task SoftDeleteAsync(int id);
}
