using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;

namespace SRS.Application.Features.ManualBilling.GetManualBillInvoice;

public class GetManualBillInvoiceHandler(IManualBillRepository repository) : IGetManualBillInvoiceHandler
{
    public async Task<ManualBillInvoiceDto?> Handle(GetManualBillInvoiceQuery query, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByBillNumberAsync(query.BillNumber, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    private static ManualBillInvoiceDto Map(ManualBill e)
    {
        return new ManualBillInvoiceDto
        {
            BillNumber = e.BillNumber,
            SaleDate = e.CreatedAtUtc,
            DeliveryTime = null,
            CustomerName = e.CustomerName,
            FatherName = null,
            Phone = e.Phone,
            Address = e.Address,
            PhotoUrl = e.PhotoUrl,
            IdProofNumber = null,
            CustomerPhone = e.Phone,
            CustomerAddress = e.Address,
            CustomerPhotoUrl = e.PhotoUrl,
            VehicleBrand = "Manual",
            VehicleModel = e.ItemDescription,
            RegistrationNumber = "N/A",
            ChassisNumber = e.ChassisNumber,
            EngineNumber = e.EngineNumber,
            Colour = e.Color,
            SellingPrice = e.AmountTotal,
            PaymentMode = e.PaymentMode,
            CashAmount = e.CashAmount,
            UpiAmount = e.UpiAmount,
            FinanceAmount = e.FinanceAmount,
            FinanceCompany = e.FinanceCompany,
            RcBookReceived = false,
            OwnershipTransferAccepted = false,
            VehicleAcceptedInAsIsCondition = false,
            Profit = 0
        };
    }
}
