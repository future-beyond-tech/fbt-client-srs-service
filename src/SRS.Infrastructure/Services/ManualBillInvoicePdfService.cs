using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Application.Mapping;
using SRS.Domain.Entities;
using SRS.Domain.Interfaces;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

/// <summary>
/// Generates delivery note PDF for manual bills using the shared wkhtmltopdf pipeline.
/// Idempotent: reuses cached PDF URL when already generated.
/// </summary>
public sealed class ManualBillInvoicePdfService(
    AppDbContext context,
    ICloudStorageService cloudStorageService,
    IDeliveryNoteSettingsService deliveryNoteSettingsService,
    IDeliveryNoteWkHtmlPdfGenerator pdfGenerator,
    ILogger<ManualBillInvoicePdfService> logger) : IManualBillInvoicePdfService
{
    public async Task<string> GetOrCreatePdfUrlAsync(int billNumber, CancellationToken cancellationToken = default)
    {
        var bill = await GetBillOrThrowAsync(billNumber, cancellationToken);

        if (!string.IsNullOrWhiteSpace(bill.InvoicePdfUrl))
        {
            if (!bill.InvoiceGeneratedAt.HasValue)
            {
                bill.InvoiceGeneratedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }
            logger.LogDebug("Reusing existing PDF URL for manual bill {BillNumber}.", billNumber);
            return bill.InvoicePdfUrl;
        }

        var (_, pdfUrl) = await GenerateAndStorePdfAsync(bill, cancellationToken);
        return pdfUrl;
    }

    public async Task<byte[]> GetPdfBytesAsync(int billNumber, CancellationToken cancellationToken = default)
    {
        var bill = await GetBillOrThrowAsync(billNumber, cancellationToken);

        if (!string.IsNullOrWhiteSpace(bill.InvoicePdfUrl))
        {
            var fetched = await TryFetchPdfBytesFromUrlAsync(bill.InvoicePdfUrl, cancellationToken);
            if (fetched is { Length: > 0 })
                return fetched;
        }

        var (pdfBytes, _) = await GenerateAndStorePdfAsync(bill, cancellationToken);
        return pdfBytes;
    }

    private async Task<ManualBill> GetBillOrThrowAsync(int billNumber, CancellationToken cancellationToken)
    {
        var bill = await context.ManualBills
            .FirstOrDefaultAsync(b => b.BillNumber == billNumber, cancellationToken);
        if (bill is null)
            throw new KeyNotFoundException("Manual bill not found.");
        return bill;
    }

    private async Task<(byte[] PdfBytes, string PdfUrl)> GenerateAndStorePdfAsync(ManualBill bill, CancellationToken cancellationToken)
    {
        var settings = await deliveryNoteSettingsService.GetAsync();
        var pdfVm = ManualInvoiceTemplateMapper.ToPdfViewModel(bill, settings);
        var vm = ManualBillPdfMapper.ToTemplateViewModel(pdfVm, settings);

        var photoDataUrl = await GetPhotoDataUrlAsync(bill.PhotoUrl, cancellationToken);
        var pdfBytes = await pdfGenerator.GenerateFromTemplateAsync(vm, photoDataUrl, cancellationToken);

        var fileName = $"manual-invoice-{bill.BillNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var pdfUrl = await cloudStorageService.UploadPdfAsync(pdfBytes, fileName, cancellationToken);

        bill.InvoicePdfUrl = pdfUrl;
        bill.InvoiceGeneratedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Manual bill PDF generated for bill {BillNumber}. URL stored.", bill.BillNumber);
        return (pdfBytes, pdfUrl);
    }

    private static async Task<byte[]?> TryFetchPdfBytesFromUrlAsync(string pdfUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(pdfUrl)) return null;
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(pdfUrl, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return bytes is { Length: > 0 } ? bytes : null;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> GetPhotoDataUrlAsync(string? photoUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(photoUrl)) return null;
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(photoUrl, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            if (bytes is not { Length: > 0 }) return null;
            return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
        }
        catch
        {
            return null;
        }
    }
}
