using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponseDto> CreateAsync(SaleCreateDto dto);
    Task<PagedResult<SaleHistoryDto>> GetHistoryAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    Task<BillDetailDto?> GetByBillNumberAsync(int billNumber);
    Task<SaleInvoiceDto?> GetInvoiceAsync(int billNumber);
    Task<SendInvoiceResponseDto> SendInvoiceAsync(int billNumber, CancellationToken cancellationToken = default);
    Task<ProcessInvoiceResponseDto> ProcessInvoiceAsync(int billNumber, CancellationToken cancellationToken = default);
}
