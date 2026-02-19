using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using SRS.Infrastructure.Persistence;

namespace SRS.Infrastructure.Services;

public class SaleService(
    AppDbContext context,
    IInvoicePdfService invoicePdfService,
    ICloudinaryService cloudinaryService,
    IWhatsAppService whatsAppService) : ISaleService
{
    private static readonly Regex E164PhoneRegex = new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled);

    public async Task<SaleResponseDto> CreateAsync(SaleCreateDto dto)
    {
        ValidateRequest(dto);

        await using var transaction =
            await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var vehicle = await context.Vehicles
            .Include(v => v.Purchase)
            .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

        if (vehicle is null)
        {
            throw new KeyNotFoundException("Vehicle not found.");
        }

        if (vehicle.Purchase is null)
        {
            throw new InvalidOperationException("Purchase record not found for the vehicle.");
        }

        if (vehicle.Status != VehicleStatus.Available)
        {
            throw new InvalidOperationException("Vehicle already sold.");
        }

        var hasExistingSale = await context.Sales
            .AnyAsync(s => s.VehicleId == dto.VehicleId);

        if (hasExistingSale)
        {
            throw new InvalidOperationException("Vehicle already sold.");
        }

        var billNumber = await GenerateBillNumberAsync();
        var cashAmount = dto.CashAmount ?? 0m;
        var upiAmount = dto.UpiAmount ?? 0m;
        var financeAmount = dto.FinanceAmount ?? 0m;
        var totalReceived = cashAmount + upiAmount + financeAmount;
        var profit = vehicle.SellingPrice - (vehicle.Purchase.BuyingCost + vehicle.Purchase.Expense);

        var customer = await ResolveCustomerForSaleAsync(dto);

        var sale = new Sale
        {
            BillNumber = billNumber,
            VehicleId = vehicle.Id,
            CustomerId = customer.Id,
            PaymentMode = dto.PaymentMode,
            CashAmount = dto.CashAmount,
            UpiAmount = dto.UpiAmount,
            FinanceAmount = dto.FinanceAmount,
            FinanceCompany = string.IsNullOrWhiteSpace(dto.FinanceCompany) ? null : dto.FinanceCompany.Trim(),
            SaleDate = dto.SaleDate,
            Profit = profit
        };

        vehicle.Status = VehicleStatus.Sold;
        context.Sales.Add(sale);

        try
        {
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraint(ex, "BillNumber"))
        {
            throw new InvalidOperationException("Bill number collision occurred. Please retry.", ex);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraint(ex, "VehicleId"))
        {
            throw new InvalidOperationException("Vehicle already sold.", ex);
        }

        return new SaleResponseDto
        {
            BillNumber = sale.BillNumber,
            VehicleId = vehicle.Id,
            Vehicle = $"{vehicle.Brand} {vehicle.Model}",
            CustomerName = customer.Name,
            TotalReceived = totalReceived,
            Profit = sale.Profit,
            SaleDate = sale.SaleDate
        };
    }

    public async Task<PagedResult<SaleHistoryDto>> GetHistoryAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Vehicle)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var toExclusive = toDate.Value.Date.AddDays(1);
            query = query.Where(s => s.SaleDate < toExclusive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            var likePattern = $"%{normalizedSearch}%";
            var parsedBill = int.TryParse(normalizedSearch, out var billNumber) ? billNumber : (int?)null;

            query = query.Where(s =>
                (parsedBill.HasValue && s.BillNumber == parsedBill.Value) ||
                EF.Functions.ILike(s.Customer.Name, likePattern) ||
                EF.Functions.ILike(s.Customer.Phone, likePattern) ||
                EF.Functions.ILike(s.Vehicle.Model, likePattern) ||
                EF.Functions.ILike(s.Vehicle.RegistrationNumber, likePattern));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SaleHistoryDto
            {
                BillNumber = s.BillNumber,
                SaleDate = s.SaleDate,
                CustomerName = s.Customer.Name,
                Phone = s.Customer.Phone,
                VehicleModel = s.Vehicle.Model,
                RegistrationNumber = s.Vehicle.RegistrationNumber,
                Profit = s.Profit
            })
            .ToListAsync();

        return new PagedResult<SaleHistoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public Task<BillDetailDto?> GetByBillNumberAsync(int billNumber)
    {
        return context.Sales
            .AsNoTracking()
            .Include(s => s.Vehicle)
            .ThenInclude(v => v.Purchase)
            .Include(s => s.Customer)
            .Where(s => s.BillNumber == billNumber)
            .Select(s => new BillDetailDto
            {
                BillNumber = s.BillNumber,
                SaleDate = s.SaleDate,
                VehicleId = s.VehicleId,
                Brand = s.Vehicle.Brand,
                Model = s.Vehicle.Model,
                Year = s.Vehicle.Year,
                RegistrationNumber = s.Vehicle.RegistrationNumber,
                ChassisNumber = s.Vehicle.ChassisNumber,
                EngineNumber = s.Vehicle.EngineNumber,
                SellingPrice = s.Vehicle.SellingPrice,
                CustomerName = s.Customer.Name,
                CustomerPhone = s.Customer.Phone,
                CustomerAddress = s.Customer.Address,
                CustomerPhotoUrl = s.Customer.PhotoUrl,
                PurchaseDate = s.Vehicle.Purchase.PurchaseDate,
                BuyingCost = s.Vehicle.Purchase.BuyingCost,
                Expense = s.Vehicle.Purchase.Expense,
                Colour = s.Vehicle.Colour,
                PaymentMode = s.PaymentMode,
                CashAmount = s.CashAmount,
                UpiAmount = s.UpiAmount,
                FinanceAmount = s.FinanceAmount,
                FinanceCompany = s.FinanceCompany,
                Profit = s.Profit,
                TotalReceived = (s.CashAmount ?? 0m) + (s.UpiAmount ?? 0m) + (s.FinanceAmount ?? 0m)
            })
            .FirstOrDefaultAsync();
    }

    public Task<SaleInvoiceDto?> GetInvoiceAsync(int billNumber)
    {
        return BuildInvoiceQuery(billNumber)
            .Select(x => x.Invoice)
            .FirstOrDefaultAsync();
    }

    public async Task<SendInvoiceResponseDto> SendInvoiceAsync(int billNumber, CancellationToken cancellationToken = default)
    {
        var invoiceResult = await BuildInvoiceQuery(billNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (invoiceResult is null)
        {
            throw new KeyNotFoundException("Invoice not found.");
        }

        var mediaUrl = string.Empty;
        var normalizedPhone = invoiceResult.CustomerPhone;

        try
        {
            normalizedPhone = NormalizePhone(invoiceResult.CustomerPhone);

            var pdfBytes = await invoicePdfService.GenerateAsync(invoiceResult.Invoice, cancellationToken);
            var fileName = $"invoice-{invoiceResult.Invoice.BillNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            mediaUrl = await cloudinaryService.UploadPdfAsync(pdfBytes, fileName, cancellationToken);

            await whatsAppService.SendInvoiceAsync(
                normalizedPhone,
                invoiceResult.Invoice.CustomerName,
                mediaUrl,
                cancellationToken);

            await SaveWhatsAppMessageAsync(
                invoiceResult.CustomerId,
                invoiceResult.SaleId,
                normalizedPhone,
                mediaUrl,
                "Sent",
                cancellationToken);

            return new SendInvoiceResponseDto
            {
                BillNumber = invoiceResult.Invoice.BillNumber,
                PdfUrl = mediaUrl,
                Status = "Sent"
            };
        }
        catch
        {
            await SaveWhatsAppMessageAsync(
                invoiceResult.CustomerId,
                invoiceResult.SaleId,
                normalizedPhone,
                mediaUrl,
                "Failed",
                cancellationToken);

            throw;
        }
    }

    private IQueryable<SaleInvoiceQueryResult> BuildInvoiceQuery(int billNumber)
    {
        return context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Vehicle)
            .Where(s => s.BillNumber == billNumber)
            .Select(s => new SaleInvoiceQueryResult
            {
                SaleId = s.Id,
                CustomerId = s.CustomerId,
                CustomerPhone = s.Customer.Phone,
                Invoice = new SaleInvoiceDto
                {
                    BillNumber = s.BillNumber,
                    SaleDate = s.SaleDate,
                    DeliveryTime = s.DeliveryTime,
                    CustomerName = s.Customer.Name,
                    FatherName = null,
                    Phone = s.Customer.Phone,
                    Address = s.Customer.Address,
                    PhotoUrl = s.Customer.PhotoUrl ?? string.Empty,
                    IdProofNumber = null,
                    CustomerPhone = s.Customer.Phone,
                    CustomerAddress = s.Customer.Address,
                    CustomerPhotoUrl = s.Customer.PhotoUrl ?? string.Empty,
                    VehicleBrand = s.Vehicle.Brand,
                    VehicleModel = s.Vehicle.Model,
                    RegistrationNumber = s.Vehicle.RegistrationNumber,
                    ChassisNumber = s.Vehicle.ChassisNumber,
                    EngineNumber = s.Vehicle.EngineNumber,
                    Colour = s.Vehicle.Colour,
                    SellingPrice = s.Vehicle.SellingPrice,
                    PaymentMode = s.PaymentMode,
                    CashAmount = s.CashAmount,
                    UpiAmount = s.UpiAmount,
                    FinanceAmount = s.FinanceAmount,
                    FinanceCompany = s.FinanceCompany,
                    Profit = s.Profit
                }
            });
    }

    private async Task SaveWhatsAppMessageAsync(
        Guid customerId,
        int saleId,
        string phoneNumber,
        string mediaUrl,
        string status,
        CancellationToken cancellationToken)
    {
        context.WhatsAppMessages.Add(new WhatsAppMessage
        {
            CustomerId = customerId,
            SaleId = saleId,
            PhoneNumber = phoneNumber,
            MediaUrl = mediaUrl,
            Status = status,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Customer> ResolveCustomerForSaleAsync(SaleCreateDto dto)
    {
        var customerPhotoUrl = dto.CustomerPhotoUrl.Trim();

        if (dto.CustomerId.HasValue)
        {
            var existingCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId.Value);

            if (existingCustomer is null)
            {
                throw new KeyNotFoundException("Customer not found.");
            }

            existingCustomer.PhotoUrl = customerPhotoUrl;
            return existingCustomer;
        }

        var newCustomer = new Customer
        {
            Name = dto.CustomerName!.Trim(),
            Phone = dto.CustomerPhone!.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.CustomerAddress) ? null : dto.CustomerAddress.Trim(),
            PhotoUrl = customerPhotoUrl,
            CreatedAt = DateTime.UtcNow
        };

        context.Customers.Add(newCustomer);
        return newCustomer;
    }

    private async Task<int> GenerateBillNumberAsync()
    {
        var maxBillNumber = await context.Sales
            .MaxAsync(s => (int?)s.BillNumber);

        return (maxBillNumber ?? 0) + 1;
    }

    private static string NormalizePhone(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Customer phone is required.");
        }

        var normalized = phoneNumber.Trim();

        if (normalized.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["whatsapp:".Length..];
        }

        normalized = normalized.Replace(" ", string.Empty).Replace("-", string.Empty);

        if (!normalized.StartsWith('+'))
        {
            normalized = $"+{normalized}";
        }

        if (!E164PhoneRegex.IsMatch(normalized))
        {
            throw new ArgumentException("Invalid customer phone format.");
        }

        return normalized;
    }

    private void ValidateRequest(SaleCreateDto dto)
    {
        if (dto.VehicleId <= 0)
        {
            throw new ArgumentException("VehicleId is required.");
        }

        if (dto.CustomerId.HasValue && dto.CustomerId.Value == Guid.Empty)
        {
            throw new ArgumentException("CustomerId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(dto.CustomerPhotoUrl))
        {
            throw new ArgumentException("Customer photo is required during sale.");
        }

        if (!dto.CustomerId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName) ||
                string.IsNullOrWhiteSpace(dto.CustomerPhone))
            {
                throw new ArgumentException("CustomerName and CustomerPhone are required.");
            }
        }

        if (!Enum.IsDefined(dto.PaymentMode))
        {
            throw new ArgumentException("Invalid payment mode.");
        }

        if (dto.SaleDate == default)
        {
            throw new ArgumentException("SaleDate is required.");
        }

        var cashAmount = dto.CashAmount ?? 0m;
        var upiAmount = dto.UpiAmount ?? 0m;
        var financeAmount = dto.FinanceAmount ?? 0m;

        if (cashAmount < 0 || upiAmount < 0 || financeAmount < 0)
        {
            throw new ArgumentException("Payment amounts cannot be negative.");
        }

        var totalReceived = cashAmount + upiAmount + financeAmount;
        if (totalReceived <= 0)
        {
            throw new ArgumentException("At least one payment amount must be greater than zero.");
        }

        switch (dto.PaymentMode)
        {
            case PaymentMode.Cash when cashAmount <= 0 || upiAmount != 0 || financeAmount != 0:
                throw new ArgumentException("Cash mode requires only CashAmount.");
            case PaymentMode.UPI when upiAmount <= 0 || cashAmount != 0 || financeAmount != 0:
                throw new ArgumentException("Upi mode requires only UpiAmount.");
            case PaymentMode.Finance when financeAmount <= 0 || cashAmount != 0 || upiAmount != 0:
                throw new ArgumentException("Finance mode requires only FinanceAmount.");
        }

        if (financeAmount > 0 && string.IsNullOrWhiteSpace(dto.FinanceCompany))
        {
            throw new ArgumentException("FinanceCompany is required when FinanceAmount is used.");
        }
    }

    private static bool IsUniqueConstraint(DbUpdateException ex, string keyName)
    {
        return ex.InnerException?.Message.Contains(keyName, StringComparison.OrdinalIgnoreCase) == true;
    }

    private sealed class SaleInvoiceQueryResult
    {
        public int SaleId { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerPhone { get; init; } = null!;
        public SaleInvoiceDto Invoice { get; init; } = null!;
    }
}
