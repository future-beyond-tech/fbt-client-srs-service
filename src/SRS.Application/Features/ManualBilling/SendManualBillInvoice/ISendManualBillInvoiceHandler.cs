using SRS.Application.DTOs;

namespace SRS.Application.Features.ManualBilling.SendManualBillInvoice;

public interface ISendManualBillInvoiceHandler
{
    Task<SendInvoiceResponseDto> Handle(SendManualBillInvoiceCommand command, CancellationToken cancellationToken = default);
}
