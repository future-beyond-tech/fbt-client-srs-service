using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class InvoicePdfService(
    AppDbContext context,
    HttpClient httpClient,
    IDeliveryNoteSettingsService deliveryNoteSettingsService,
    IFileStorageService fileStorageService,
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

    public async Task<string> GenerateInvoiceAsync(int billNumber)
    {
        var sale = await context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.BillNumber == billNumber);

        if (sale is null)
        {
            throw new KeyNotFoundException("Invoice not found.");
        }

        if (!string.IsNullOrWhiteSpace(sale.InvoicePdfUrl))
        {
            if (!sale.InvoiceGeneratedAt.HasValue)
            {
                sale.InvoiceGeneratedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            return sale.InvoicePdfUrl;
        }

        var invoice = BuildInvoiceDto(sale);
        var pdfBytes = await GenerateAsync(invoice);
        var fileName = $"invoice-{billNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        await using var invoiceStream = new MemoryStream(pdfBytes, writable: false);
        var pdfUrl = await fileStorageService.UploadAsync(invoiceStream, fileName);

        sale.InvoicePdfUrl = pdfUrl;
        sale.InvoiceGeneratedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Invoice PDF generated for bill {BillNumber}. URL: {PdfUrl}", billNumber, pdfUrl);
        return pdfUrl;
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

    private static SaleInvoiceDto BuildInvoiceDto(Sale sale)
    {
        return new SaleInvoiceDto
        {
            BillNumber = sale.BillNumber,
            SaleDate = sale.SaleDate,
            DeliveryTime = sale.DeliveryTime,
            CustomerName = sale.Customer.Name,
            FatherName = null,
            Phone = sale.Customer.Phone,
            Address = sale.Customer.Address,
            PhotoUrl = sale.Customer.PhotoUrl ?? string.Empty,
            IdProofNumber = null,
            CustomerPhone = sale.Customer.Phone,
            CustomerAddress = sale.Customer.Address,
            CustomerPhotoUrl = sale.Customer.PhotoUrl ?? string.Empty,
            VehicleBrand = sale.Vehicle.Brand,
            VehicleModel = sale.Vehicle.Model,
            RegistrationNumber = sale.Vehicle.RegistrationNumber,
            ChassisNumber = sale.Vehicle.ChassisNumber,
            EngineNumber = sale.Vehicle.EngineNumber,
            Colour = sale.Vehicle.Colour,
            SellingPrice = sale.Vehicle.SellingPrice,
            PaymentMode = sale.PaymentMode,
            CashAmount = sale.CashAmount,
            UpiAmount = sale.UpiAmount,
            FinanceAmount = sale.FinanceAmount,
            FinanceCompany = sale.FinanceCompany,
            RcBookReceived = sale.RcBookReceived,
            OwnershipTransferAccepted = sale.OwnershipTransferAccepted,
            VehicleAcceptedInAsIsCondition = sale.VehicleAcceptedInAsIsCondition,
            Profit = sale.Profit
        };
    }
}
