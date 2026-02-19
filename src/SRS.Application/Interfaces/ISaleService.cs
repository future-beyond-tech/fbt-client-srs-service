using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponseDto> CreateAsync(SaleCreateDto dto);
    Task<BillDetailDto?> GetByBillNumberAsync(int billNumber);
}
