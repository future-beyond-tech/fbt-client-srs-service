using Microsoft.Extensions.Logging;
using SRS.Application.Common;
using SRS.Application.DTOs;
using SRS.Application.Interfaces;

namespace SRS.Application.Features.ManualBilling.SendManualBillInvoice;

public sealed class SendManualBillInvoiceHandler(
    IManualBillRepository repository,
    IManualBillInvoicePdfService pdfService,
    IWhatsAppService whatsAppService,
    ILogger<SendManualBillInvoiceHandler> logger) : ISendManualBillInvoiceHandler
{
    public async Task<SendInvoiceResponseDto> Handle(SendManualBillInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var bill = await repository.GetByBillNumberAsync(command.BillNumber, cancellationToken);
        if (bill is null)
            throw new KeyNotFoundException("Manual bill not found.");

        if (string.IsNullOrWhiteSpace(bill.Phone))
            throw new ArgumentException("Customer phone is required to send invoice.");

        var normalizedPhone = PhoneNormalizer.NormalizeToE164(bill.Phone);
        var pdfUrl = await pdfService.GetOrCreatePdfUrlAsync(command.BillNumber, cancellationToken);

        await whatsAppService.SendInvoiceAsync(
            normalizedPhone,
            bill.CustomerName,
            pdfUrl,
            cancellationToken);

        var maskedPhone = PhoneMask.MaskLastFour(normalizedPhone);
        logger.LogInformation(
            "Manual bill invoice sent via WhatsApp. BillNumber={BillNumber}, Phone={MaskedPhone}",
            command.BillNumber,
            maskedPhone);

        return new SendInvoiceResponseDto
        {
            BillNumber = command.BillNumber,
            PdfUrl = pdfUrl,
            Status = "Sent"
        };
    }
}
