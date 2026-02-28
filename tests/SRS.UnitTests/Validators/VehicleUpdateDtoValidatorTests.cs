using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Validators;
using Xunit;

namespace SRS.UnitTests.Validators;

public sealed class VehicleUpdateDtoValidatorTests
{
    private readonly VehicleUpdateDtoValidator _sut = new();

    [Fact]
    public void ValidDto_Passes()
    {
        var dto = new VehicleUpdateDto
        {
            SellingPrice = 100_000,
            Colour = "White",
            RegistrationNumber = "REG123"
        };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void SellingPrice_Negative_Fails(decimal value)
    {
        var dto = new VehicleUpdateDto { SellingPrice = value };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(VehicleUpdateDto.SellingPrice));
    }

    [Fact]
    public void SellingPrice_Zero_Passes()
    {
        var dto = new VehicleUpdateDto { SellingPrice = 0 };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegistrationNumber_EmptyWhenProvided_Fails()
    {
        var dto = new VehicleUpdateDto
        {
            SellingPrice = 100,
            RegistrationNumber = "   "
        };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(VehicleUpdateDto.RegistrationNumber));
    }

    [Fact]
    public void Colour_OverMaxLength_Fails()
    {
        var dto = new VehicleUpdateDto
        {
            SellingPrice = 100,
            Colour = new string('x', 51)
        };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
    }
}
