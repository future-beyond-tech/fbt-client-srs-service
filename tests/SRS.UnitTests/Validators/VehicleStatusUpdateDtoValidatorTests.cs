using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Validators;
using SRS.Domain.Enums;
using Xunit;

namespace SRS.UnitTests.Validators;

public sealed class VehicleStatusUpdateDtoValidatorTests
{
    private readonly VehicleStatusUpdateDtoValidator _sut = new();

    [Fact]
    public void ValidStatus_Available_Passes()
    {
        var dto = new VehicleStatusUpdateDto { Status = VehicleStatus.Available };
        _sut.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidStatus_Sold_Passes()
    {
        var dto = new VehicleStatusUpdateDto { Status = VehicleStatus.Sold };
        _sut.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void InvalidStatus_OutOfRange_Fails()
    {
        var dto = new VehicleStatusUpdateDto { Status = (VehicleStatus)99 };
        _sut.Validate(dto).IsValid.Should().BeFalse();
    }
}
