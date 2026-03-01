using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Validators;
using Xunit;

namespace SRS.UnitTests.Validators;

public sealed class PurchaseExpenseCreateDtoValidatorTests
{
    private readonly PurchaseExpenseCreateDtoValidator _sut = new();

    [Fact]
    public void ValidDto_Passes()
    {
        var dto = new PurchaseExpenseCreateDto { ExpenseType = "Repair", Amount = 100 };
        _sut.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ExpenseType_Empty_Fails()
    {
        var dto = new PurchaseExpenseCreateDto { ExpenseType = "", Amount = 100 };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PurchaseExpenseCreateDto.ExpenseType));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Amount_NotPositive_Fails(decimal amount)
    {
        var dto = new PurchaseExpenseCreateDto { ExpenseType = "Repair", Amount = amount };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PurchaseExpenseCreateDto.Amount));
    }

    [Fact]
    public void ExpenseType_OverMaxLength_Fails()
    {
        var dto = new PurchaseExpenseCreateDto { ExpenseType = new string('x', 101), Amount = 1 };
        _sut.Validate(dto).IsValid.Should().BeFalse();
    }
}
