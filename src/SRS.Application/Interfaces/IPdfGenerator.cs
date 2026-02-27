using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IPdfGenerator
{
    Task<byte[]> GeneratePdfAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default);
}
