using SRS.Application.DTOs;

namespace SRS.Application.Interfaces;

/// <summary>
/// Generates Delivery Note PDF from the shared template view model using HTML → wkhtmltopdf CLI.
/// Used by both Sales and Manual Billing for a single engine and consistent layout.
/// </summary>
public interface IDeliveryNoteWkHtmlPdfGenerator
{
    /// <summary>Renders template VM to PDF. Photo may be null (placeholder shown). Requires wkhtmltopdf CLI.</summary>
    Task<byte[]> GenerateFromTemplateAsync(DeliveryNoteTemplateViewModel vm, string? photoDataUrl, CancellationToken cancellationToken = default);
}
