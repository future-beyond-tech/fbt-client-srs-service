using Microsoft.Extensions.Logging;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Application.Mapping;
using SRS.Infrastructure.PdfTemplate;

namespace SRS.Infrastructure.Services;

/// <summary>
/// Generates Delivery Note PDF using HTML template + wkhtmltopdf CLI.
/// Single engine for both Sales and Manual Billing. Requires wkhtmltopdf CLI (no DinkToPdf native lib).
/// </summary>
public sealed class DeliveryNoteWkHtmlPdfGenerator(
    IWkhtmltopdfCliGenerator cliGenerator,
    IDeliveryNoteSettingsService settingsService,
    ILogger<DeliveryNoteWkHtmlPdfGenerator> logger) : IManualBillHtmlPdfGenerator, IDeliveryNoteWkHtmlPdfGenerator
{
    public async Task<byte[]> GenerateAsync(ManualBillPdfViewModel viewModel, string? customerPhotoUrl, CancellationToken cancellationToken = default)
    {
        if (viewModel == null)
            throw new ArgumentNullException(nameof(viewModel));
        cancellationToken.ThrowIfCancellationRequested();

        var settings = await settingsService.GetAsync();
        var vm = ManualBillPdfMapper.ToTemplateViewModel(viewModel, settings);

        string? photoDataUrl = null;
        if (!string.IsNullOrWhiteSpace(customerPhotoUrl))
        {
            var bytes = await TryDownloadImageAsync(customerPhotoUrl, cancellationToken);
            if (bytes is { Length: > 0 })
                photoDataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
        }

        return await GenerateFromTemplateAsync(vm, photoDataUrl, cancellationToken);
    }

    public async Task<byte[]> GenerateFromTemplateAsync(DeliveryNoteTemplateViewModel vm, string? photoDataUrl, CancellationToken cancellationToken = default)
    {
        if (vm == null)
            throw new ArgumentNullException(nameof(vm));
        cancellationToken.ThrowIfCancellationRequested();

        var fontBase64 = TryLoadNotoSansTamilBase64();
        var html = DeliveryNoteHtmlBuilder.Build(vm, photoDataUrl, fontBase64);

        var pdfBytes = await cliGenerator.GeneratePdfAsync(html, cancellationToken);

        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            logger.LogError("wkhtmltopdf returned empty bytes for bill {BillNumber}.", vm.BillNumber);
            throw new InvalidOperationException("PDF generation returned no data.");
        }

        return pdfBytes;
    }

    private static async Task<byte[]?> TryDownloadImageAsync(string url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync(ct);
        }
        catch
        {
            return null;
        }
    }

    private static string? TryLoadNotoSansTamilBase64()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "Infrastructure", "Services", "Pdf", "Fonts", "NotoSansTamil-Regular.ttf");
            if (!File.Exists(path)) return null;
            var bytes = File.ReadAllBytes(path);
            return Convert.ToBase64String(bytes);
        }
        catch
        {
            return null;
        }
    }
}
