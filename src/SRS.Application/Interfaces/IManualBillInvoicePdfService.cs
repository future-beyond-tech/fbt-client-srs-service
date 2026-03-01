namespace SRS.Application.Interfaces;

/// <summary>
/// Generates or returns cached delivery note PDF URL for a manual bill.
/// Idempotent: if PDF already generated, returns existing URL without regenerating.
/// </summary>
public interface IManualBillInvoicePdfService
{
    /// <summary>Returns existing PDF URL if present; otherwise generates PDF, stores it, updates the manual bill, and returns the URL.</summary>
    Task<string> GetOrCreatePdfUrlAsync(int billNumber, CancellationToken cancellationToken = default);

    /// <summary>Returns PDF bytes (generates or downloads from stored URL). Used for GET .../pdf returning application/pdf.</summary>
    Task<byte[]> GetPdfBytesAsync(int billNumber, CancellationToken cancellationToken = default);
}
