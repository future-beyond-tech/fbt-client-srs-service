using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default);
}
