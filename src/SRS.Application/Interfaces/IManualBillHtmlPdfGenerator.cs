using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

/// <summary>
/// Generates Manual Bill Delivery Note PDF via HTML-to-PDF (wkhtmltopdf CLI).
/// Implemented by <see cref="DeliveryNoteWkHtmlPdfGenerator"/> when wkhtmltopdf CLI is available.
/// </summary>
public interface IManualBillHtmlPdfGenerator
{
    Task<byte[]> GenerateAsync(ManualBillPdfViewModel viewModel, string? customerPhotoUrl, CancellationToken cancellationToken = default);
}
