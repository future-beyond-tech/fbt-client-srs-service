using FluentAssertions;
using SRS.Application.DTOs;
using SRS.Application.Validators;
using Xunit;

namespace SRS.UnitTests.Validators;

public sealed class FinanceCompanyCreateDtoValidatorTests
{
    private readonly FinanceCompanyCreateDtoValidator _sut = new();

    [Fact]
    public void ValidDto_Passes()
    {
        var dto = new FinanceCompanyCreateDto { Name = "Test Finance Co" };
        _sut.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Name_Empty_Fails()
    {
        var dto = new FinanceCompanyCreateDto { Name = "" };
        var result = _sut.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(FinanceCompanyCreateDto.Name));
    }

    [Fact]
    public void Name_Null_Fails()
    {
        var dto = new FinanceCompanyCreateDto { Name = null! };
        _sut.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Name_OverMaxLength_Fails()
    {
        var dto = new FinanceCompanyCreateDto { Name = new string('x', 151) };
        _sut.Validate(dto).IsValid.Should().BeFalse();
    }
}
