using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Validators;
using Xunit;

namespace SRS.UnitTests.Validators;

public sealed class UpdateDeliveryNoteSettingsDtoValidatorTests
{
    private readonly UpdateDeliveryNoteSettingsDtoValidator _sut = new();

    [Fact]
    public void ValidDto_Passes()
    {
        var dto = new UpdateDeliveryNoteSettingsDto
        {
            ShopName = "Test Shop",
            ShopAddress = "Test Address"
        };
        _sut.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShopName_Empty_Fails()
    {
        var dto = new UpdateDeliveryNoteSettingsDto { ShopName = "", ShopAddress = "Addr" };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateDeliveryNoteSettingsDto.ShopName));
    }

    [Fact]
    public void ShopAddress_Empty_Fails()
    {
        var dto = new UpdateDeliveryNoteSettingsDto { ShopName = "Shop", ShopAddress = "" };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateDeliveryNoteSettingsDto.ShopAddress));
    }

    [Fact]
    public void LogoUrl_InvalidUrl_Fails()
    {
        var dto = new UpdateDeliveryNoteSettingsDto
        {
            ShopName = "Shop",
            ShopAddress = "Addr",
            LogoUrl = "not-a-valid-url"
        };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateDeliveryNoteSettingsDto.LogoUrl));
    }

    [Fact]
    public void LogoUrl_ValidAbsoluteUrl_Passes()
    {
        var dto = new UpdateDeliveryNoteSettingsDto
        {
            ShopName = "Shop",
            ShopAddress = "Addr",
            LogoUrl = "https://example.com/logo.png"
        };
        _sut.Validate(dto).IsValid.Should().BeTrue();
    }
}
