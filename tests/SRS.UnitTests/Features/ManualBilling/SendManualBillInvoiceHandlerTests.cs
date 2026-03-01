using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SRS.Application.DTOs;
using SRS.Application.Features.ManualBilling.SendManualBillInvoice;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using Xunit;

namespace SRS.UnitTests.Features.ManualBilling;

public sealed class SendManualBillInvoiceHandlerTests
{
    private readonly Mock<IManualBillRepository> _repository = new();
    private readonly Mock<IManualBillInvoicePdfService> _pdfService = new();
    private readonly Mock<IWhatsAppService> _whatsAppService = new();
    private readonly Mock<ILogger<SendManualBillInvoiceHandler>> _logger = new();
    private readonly SendManualBillInvoiceHandler _sut;

    public SendManualBillInvoiceHandlerTests()
    {
        _sut = new SendManualBillInvoiceHandler(
            _repository.Object,
            _pdfService.Object,
            _whatsAppService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_WhenBillNotFound_ThrowsKeyNotFoundException()
    {
        _repository.Setup(r => r.GetByBillNumberAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ManualBill?)null);

        var act = () => _sut.Handle(new SendManualBillInvoiceCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*not found*");
        _pdfService.Verify(p => p.GetOrCreatePdfUrlAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _whatsAppService.Verify(w => w.SendInvoiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBillExists_CallsPdfServiceThenWhatsApp_ReturnsResult()
    {
        var bill = new ManualBill
        {
            Id = 1,
            BillNumber = 1,
            CustomerName = "Test Customer",
            Phone = "+919876543210",
            PhotoUrl = "https://example.com/p.jpg",
            ItemDescription = "Item",
            AmountTotal = 100m
        };
        _repository.Setup(r => r.GetByBillNumberAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(bill);
        _pdfService.Setup(p => p.GetOrCreatePdfUrlAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/manual-invoice-1.pdf");
        _whatsAppService.Setup(w => w.SendInvoiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("message-id");

        var result = await _sut.Handle(new SendManualBillInvoiceCommand(1), CancellationToken.None);

        result.BillNumber.Should().Be(1);
        result.PdfUrl.Should().Be("https://storage.example.com/manual-invoice-1.pdf");
        result.Status.Should().Be("Sent");
        _whatsAppService.Verify(w => w.SendInvoiceAsync("+919876543210", "Test Customer", "https://storage.example.com/manual-invoice-1.pdf", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SecondCall_ReusesSamePdfUrl_FromPdfService()
    {
        var bill = new ManualBill { BillNumber = 1, CustomerName = "C", Phone = "+919876543210", PhotoUrl = "u", ItemDescription = "i", AmountTotal = 1m };
        _repository.Setup(r => r.GetByBillNumberAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(bill);
        var sameUrl = "https://storage.example.com/cached.pdf";
        _pdfService.Setup(p => p.GetOrCreatePdfUrlAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sameUrl);
        _whatsAppService.Setup(w => w.SendInvoiceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("ok");

        var r1 = await _sut.Handle(new SendManualBillInvoiceCommand(1), CancellationToken.None);
        var r2 = await _sut.Handle(new SendManualBillInvoiceCommand(1), CancellationToken.None);

        r1.PdfUrl.Should().Be(sameUrl);
        r2.PdfUrl.Should().Be(sameUrl);
        _pdfService.Verify(p => p.GetOrCreatePdfUrlAsync(1, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
