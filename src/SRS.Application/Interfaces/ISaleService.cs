using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponseDto> CreateAsync(SaleCreateDto dto);
    Task<SaleResponseDto?> GetByBillNumberAsync(string billNumber);
    Task<BillDetailDto?> GetBillDetailAsync(string billNumber);
}
