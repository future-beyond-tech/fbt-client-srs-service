using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.Application.Services;

public class InvoicePdfService(
    HttpClient httpClient,
    IDeliveryNoteSettingsService deliveryNoteSettingsService,
    IWebHostEnvironment environment,
    ILogger<InvoicePdfService> logger) : IInvoicePdfService
{
    static InvoicePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        var settings = await deliveryNoteSettingsService.GetAsync();
        var customerImageBytes = await TryDownloadImageAsync(invoice.PhotoUrl, cancellationToken);
        var logoBytes = await TryDownloadImageAsync(settings.LogoUrl, cancellationToken);

        var document = new InvoiceDocument(
            invoice,
            customerImageBytes,
            logoBytes,
            settings);

        return document.GeneratePdf();
    }

    private async Task<byte[]?> TryDownloadImageAsync(string? photoUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            return null;
        }

        if (TryResolveLocalUploadPath(photoUrl, out var localPath))
        {
            try
            {
                return await File.ReadAllBytesAsync(localPath, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to read local image from {ImagePath}.", localPath);
                return null;
            }
        }

        if (!Uri.TryCreate(photoUrl, UriKind.Absolute, out var imageUri))
        {
            return null;
        }

        try
        {
            using var response = await httpClient.GetAsync(imageUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to download image from {ImageUrl}.", photoUrl);
            return null;
        }
    }

    private bool TryResolveLocalUploadPath(string imageUrl, out string filePath)
    {
        filePath = string.Empty;
        var trimmed = imageUrl.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
        {
            return false;
        }

        if (trimmed.StartsWith("~/", StringComparison.Ordinal))
        {
            trimmed = trimmed[2..];
        }
        else
        {
            trimmed = trimmed.TrimStart('/');
        }

        if (!trimmed.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = trimmed.Replace('/', Path.DirectorySeparatorChar);
        var uploadsRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "Uploads"));
        var candidatePath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, relativePath));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (!candidatePath.StartsWith(uploadsRoot, comparison))
        {
            return false;
        }

        if (!File.Exists(candidatePath))
        {
            return false;
        }

        filePath = candidatePath;
        return true;
    }
}
