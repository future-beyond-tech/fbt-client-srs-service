using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SRS.Application.DTOs;
using SRS.Application.Features.ManualBilling.CreateManualBill;
using SRS.Application.Interfaces;
using SRS.Domain.Entities;
using SRS.Domain.Enums;
using Xunit;

namespace SRS.UnitTests.Features.ManualBilling;

public sealed class CreateManualBillHandlerTests
{
    private readonly Mock<IManualBillRepository> _repository = new();
    private readonly Mock<FluentValidation.IValidator<CreateManualBillCommand>> _validator = new();
    private readonly Mock<ILogger<CreateManualBillHandler>> _logger = new();
    private readonly CreateManualBillHandler _sut;

    public CreateManualBillHandlerTests()
    {
        _validator.Setup(v => v.ValidateAsync(It.IsAny<CreateManualBillCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _sut = new CreateManualBillHandler(_repository.Object, _validator.Object, _logger.Object);
    }

    private static ManualBillCreateDto ValidDto() => new()
    {
        CustomerName = "Unit Test Customer",
        Phone = "9876543210",
        Address = "Unit Test Address",
        PhotoUrl = "https://storage.example.com/photo.jpg",
        ItemDescription = "Item description",
        AmountTotal = 500m,
        PaymentMode = PaymentMode.UPI,
        CashAmount = null,
        UpiAmount = 500m,
        FinanceAmount = null,
        FinanceCompany = null
    };

    [Fact]
    public async Task Handle_ValidCommand_ReturnsResultWithBillNumber()
    {
        var added = new ManualBill
        {
            Id = 1,
            BillNumber = 1,
            BillType = "Manual",
            CustomerName = "Unit Test Customer",
            Phone = "+919876543210",
            PhotoUrl = "https://storage.example.com/photo.jpg",
            ItemDescription = "Item description",
            AmountTotal = 500m,
            PaymentMode = PaymentMode.UPI,
            UpiAmount = 500m,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _repository.Setup(r => r.GetNextBillNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repository.Setup(r => r.AddAsync(It.IsAny<ManualBill>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(added);

        var command = new CreateManualBillCommand(ValidDto());
        var result = await _sut.Handle(command, CancellationToken.None);

        result.BillNumber.Should().Be(1);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.PdfUrl.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NormalizesPhone_ToE164()
    {
        var dto = ValidDto();
        dto.Phone = "9876543210";
        ManualBill? captured = null;
        _repository.Setup(r => r.GetNextBillNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repository.Setup(r => r.AddAsync(It.IsAny<ManualBill>(), It.IsAny<CancellationToken>()))
            .Callback<ManualBill, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((ManualBill e, CancellationToken _) => e);

        await _sut.Handle(new CreateManualBillCommand(dto), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Phone.Should().Be("+919876543210");
    }

    [Fact]
    public async Task Handle_ValidationFails_ThrowsValidationException()
    {
        _validator.Setup(v => v.ValidateAsync(It.IsAny<CreateManualBillCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                new[] { new FluentValidation.Results.ValidationFailure("Dto.PhotoUrl", "Photo URL is required.") }));

        var act = () => _sut.Handle(new CreateManualBillCommand(ValidDto()), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        _repository.Verify(r => r.AddAsync(It.IsAny<ManualBill>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
