using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Interfaces;
using SRS.Infrastructure.Persistence;

namespace SRS.Application.Services;

public class InvoicePdfService(
    AppDbContext context,
    ICloudStorageService cloudStorageService,
    IPdfGenerator pdfGenerator,
    ILogger<InvoicePdfService> logger) : IInvoicePdfService
{
    static InvoicePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAsync(SaleInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        return await pdfGenerator.GeneratePdfAsync(invoice, cancellationToken);
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
        var pdfBytes = await pdfGenerator.GeneratePdfAsync(invoice);
        var fileName = $"invoice-{billNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

        var pdfUrl = await cloudStorageService.UploadPdfAsync(pdfBytes, fileName);

        sale.InvoicePdfUrl = pdfUrl;
        sale.InvoiceGeneratedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Invoice PDF generated for bill {BillNumber}. URL: {PdfUrl}", billNumber, pdfUrl);
        return pdfUrl;
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
