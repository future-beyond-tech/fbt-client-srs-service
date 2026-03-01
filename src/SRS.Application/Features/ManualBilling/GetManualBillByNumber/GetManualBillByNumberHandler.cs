using SRS.Application.DTOs;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;

namespace SRS.Application.Features.ManualBilling.GetManualBillByNumber;

public class GetManualBillByNumberHandler(IManualBillRepository repository) : IGetManualBillByNumberHandler
{
    public async Task<ManualBillDetailDto?> Handle(GetManualBillByNumberQuery query, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByBillNumberAsync(query.BillNumber, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    private static ManualBillDetailDto Map(ManualBill e)
    {
        return new ManualBillDetailDto
        {
            BillNumber = e.BillNumber,
            BillType = e.BillType,
            CustomerName = e.CustomerName,
            Phone = e.Phone,
            Address = e.Address,
            PhotoUrl = e.PhotoUrl,
            SellerName = e.SellerName,
            SellerAddress = e.SellerAddress,
            CustomerNameTitle = e.CustomerNameTitle,
            SellerNameTitle = e.SellerNameTitle,
            ItemDescription = e.ItemDescription,
            ChassisNumber = e.ChassisNumber,
            EngineNumber = e.EngineNumber,
            Color = e.Color,
            Notes = e.Notes,
            AmountTotal = e.AmountTotal,
            PaymentMode = e.PaymentMode,
            CashAmount = e.CashAmount,
            UpiAmount = e.UpiAmount,
            FinanceAmount = e.FinanceAmount,
            FinanceCompany = e.FinanceCompany,
            CreatedAtUtc = e.CreatedAtUtc,
            UpdatedAtUtc = e.UpdatedAtUtc,
            InvoicePdfUrl = e.InvoicePdfUrl
        };
    }
}
