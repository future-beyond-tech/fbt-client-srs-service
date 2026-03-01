using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Application.Mapping;
using SRS.Domain.Entities;
using SRS.Domain.Interfaces;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class InvoicePdfService(
    AppDbContext context,
    ICloudStorageService cloudStorageService,
    IDeliveryNoteWkHtmlPdfGenerator pdfGenerator,
    IDeliveryNoteSettingsService deliveryNoteSettingsService,
    IHttpClientFactory httpClientFactory,
    ILogger<InvoicePdfService> logger) : IInvoicePdfService
{
    public async Task<byte[]> GenerateAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        var settings = await deliveryNoteSettingsService.GetAsync();
        var vm = SalesInvoicePdfMapper.ToTemplateViewModel(invoice, settings);
        var photoDataUrl = await GetPhotoDataUrlAsync(invoice.CustomerPhotoUrl ?? invoice.PhotoUrl, cancellationToken);
        return await pdfGenerator.GenerateFromTemplateAsync(vm, photoDataUrl, cancellationToken);
    }

    public async Task<string> GenerateInvoiceAsync(int billNumber)
    {
        var sale = await GetSaleForInvoiceAsync(billNumber, CancellationToken.None);
        if (sale is null)
            throw new KeyNotFoundException("Sale not found.");

        return await GetOrCreatePdfUrlAndUpdateSaleAsync(sale, CancellationToken.None);
    }

    public async Task<byte[]> GetPdfBytesAsync(int billNumber, CancellationToken cancellationToken = default)
    {
        var sale = await GetSaleForInvoiceAsync(billNumber, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException("Sale not found.");

        if (!string.IsNullOrWhiteSpace(sale.InvoicePdfUrl))
        {
            var bytes = await DownloadPdfFromUrlAsync(sale.InvoicePdfUrl, cancellationToken);
            if (bytes is { Length: > 0 })
            {
                logger.LogDebug("Returning stored PDF for bill {BillNumber} from storage.", billNumber);
                return bytes;
            }
            logger.LogWarning("Failed to download stored PDF for bill {BillNumber}; regenerating.", billNumber);
        }

        var settings = await deliveryNoteSettingsService.GetAsync();
        var invoice = BuildInvoiceDto(sale);
        var vm = SalesInvoicePdfMapper.ToTemplateViewModel(invoice, settings);
        var photoDataUrl = await GetPhotoDataUrlAsync(sale.Customer.PhotoUrl, cancellationToken);
        var pdfBytes = await pdfGenerator.GenerateFromTemplateAsync(vm, photoDataUrl, cancellationToken);

        var fileName = $"invoice-{billNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var pdfUrl = await cloudStorageService.UploadPdfAsync(pdfBytes, fileName, cancellationToken);

        sale.InvoicePdfUrl = pdfUrl;
        sale.InvoiceGeneratedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invoice PDF generated for bill {BillNumber}. URL stored.", billNumber);
        return pdfBytes;
    }

    private async Task<Sale?> GetSaleForInvoiceAsync(int billNumber, CancellationToken cancellationToken)
    {
        return await context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.BillNumber == billNumber, cancellationToken);
    }

    private async Task<string> GetOrCreatePdfUrlAndUpdateSaleAsync(Sale sale, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(sale.InvoicePdfUrl))
        {
            if (!sale.InvoiceGeneratedAt.HasValue)
            {
                sale.InvoiceGeneratedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }
            return sale.InvoicePdfUrl;
        }

        var settings = await deliveryNoteSettingsService.GetAsync();
        var invoice = BuildInvoiceDto(sale);
        var vm = SalesInvoicePdfMapper.ToTemplateViewModel(invoice, settings);
        var photoDataUrl = await GetPhotoDataUrlAsync(sale.Customer.PhotoUrl, cancellationToken);
        var pdfBytes = await pdfGenerator.GenerateFromTemplateAsync(vm, photoDataUrl, cancellationToken);

        var fileName = $"invoice-{sale.BillNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var pdfUrl = await cloudStorageService.UploadPdfAsync(pdfBytes, fileName, cancellationToken);

        sale.InvoicePdfUrl = pdfUrl;
        sale.InvoiceGeneratedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invoice PDF generated for bill {BillNumber}. URL: {PdfUrl}", sale.BillNumber, pdfUrl);
        return pdfUrl;
    }

    private async Task<string?> GetPhotoDataUrlAsync(string? photoUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(photoUrl)) return null;
        var bytes = await DownloadImageAsync(photoUrl, cancellationToken);
        if (bytes is not { Length: > 0 }) return null;
        return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
    }

    private async Task<byte[]?> DownloadImageAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to download image from storage.");
            return null;
        }
    }

    private async Task<byte[]?> DownloadPdfFromUrlAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to download PDF from storage.");
            return null;
        }
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
