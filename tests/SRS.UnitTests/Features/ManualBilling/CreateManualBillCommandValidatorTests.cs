using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Features.ManualBilling.CreateManualBill;
using SRS.Domain.Enums;
using Xunit;

namespace SRS.UnitTests.Features.ManualBilling;

public sealed class CreateManualBillCommandValidatorTests
{
    private readonly CreateManualBillCommandValidator _sut = new();

    private static ManualBillCreateDto ValidDto() => new()
    {
        CustomerName = "Test Customer",
        Phone = "+919876543210",
        Address = "Some Address",
        PhotoUrl = "https://example.com/photo.jpg",
        ItemDescription = "Manual entry details",
        AmountTotal = 1000m,
        PaymentMode = PaymentMode.Cash,
        CashAmount = 1000m,
        UpiAmount = null,
        FinanceAmount = null,
        FinanceCompany = null
    };

    [Fact]
    public void ValidCommand_Passes()
    {
        var command = new CreateManualBillCommand(ValidDto());
        _sut.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Dto_Null_Fails()
    {
        var command = new CreateManualBillCommand(null!);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CustomerName_Empty_Fails()
    {
        var dto = ValidDto();
        dto.CustomerName = "";
        var result = _sut.Validate(new CreateManualBillCommand(dto));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dto.CustomerName");
    }

    [Fact]
    public void PhotoUrl_Empty_Fails()
    {
        var dto = ValidDto();
        dto.PhotoUrl = "";
        var result = _sut.Validate(new CreateManualBillCommand(dto));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dto.PhotoUrl");
    }

    [Fact]
    public void ItemDescription_Empty_Fails()
    {
        var dto = ValidDto();
        dto.ItemDescription = "";
        var result = _sut.Validate(new CreateManualBillCommand(dto));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Phone_Invalid_Fails()
    {
        var dto = ValidDto();
        dto.Phone = "not-a-phone";
        var result = _sut.Validate(new CreateManualBillCommand(dto));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dto.Phone");
    }

    [Fact]
    public void Phone_Valid_10Digit_Indian_Passes()
    {
        var dto = ValidDto();
        dto.Phone = "9876543210";
        _sut.Validate(new CreateManualBillCommand(dto)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void PaymentSplit_DoesNotSumToTotal_Fails()
    {
        var dto = ValidDto();
        dto.CashAmount = 500m;
        dto.UpiAmount = 300m;
        dto.FinanceAmount = null;
        var result = _sut.Validate(new CreateManualBillCommand(dto));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dto");
    }

    [Fact]
    public void PaymentSplit_SumsToTotal_Passes()
    {
        var dto = ValidDto();
        dto.CashAmount = 400m;
        dto.UpiAmount = 600m;
        dto.FinanceAmount = null;
        _sut.Validate(new CreateManualBillCommand(dto)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void AmountTotal_Negative_Fails()
    {
        var dto = ValidDto();
        dto.AmountTotal = -1m;
        _sut.Validate(new CreateManualBillCommand(dto)).IsValid.Should().BeFalse();
    }
}
