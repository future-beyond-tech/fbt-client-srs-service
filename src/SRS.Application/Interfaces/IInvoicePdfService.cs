using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default);

    /// <summary>Get-or-create PDF URL for the sale; used by send-invoice and process-invoice. Idempotent.</summary>
    Task<string> GenerateInvoiceAsync(int billNumber);

    /// <summary>Returns PDF bytes for download. Reuses stored PDF URL if available (streams from storage); otherwise generates, stores, and returns bytes. Same pipeline as send-invoice.</summary>
    Task<byte[]> GetPdfBytesAsync(int billNumber, CancellationToken cancellationToken = default);
}
