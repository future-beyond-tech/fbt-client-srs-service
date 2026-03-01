using SRS.Application.DTOs;

namespace SRS.Application.Features.ManualBilling.GetManualBillInvoice;

public interface IGetManualBillInvoiceHandler
{
    Task<ManualBillInvoiceDto?> Handle(GetManualBillInvoiceQuery query, CancellationToken cancellationToken = default);
}
