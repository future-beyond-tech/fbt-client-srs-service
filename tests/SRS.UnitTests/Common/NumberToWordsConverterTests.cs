using FluentAssertions;
using SRS.Application.Common;
using Xunit;

namespace SRS.UnitTests.Common;

public sealed class NumberToWordsConverterTests
{
    [Fact]
    public void ToRupeesInWords_Zero_ReturnsZeroRupeesOnly()
    {
        NumberToWordsConverter.ToRupeesInWords(0).Should().Be("Zero Rupees Only");
    }

    [Fact]
    public void ToRupeesInWords_120000_ReturnsOneLakhTwentyThousand()
    {
        var result = NumberToWordsConverter.ToRupeesInWords(120000m);
        result.Should().Contain("One Lakh Twenty Thousand");
        result.Should().EndWith("Rupees Only");
    }

    [Fact]
    public void ToRupeesInWords_Negative_UsesAbsoluteValue()
    {
        var result = NumberToWordsConverter.ToRupeesInWords(-500m);
        result.Should().Contain("Five Hundred");
        result.Should().EndWith("Rupees Only");
    }

    [Fact]
    public void ToRupeesInWords_Decimal_TruncatesFractionalPart()
    {
        NumberToWordsConverter.ToRupeesInWords(99.99m).Should().Contain("Ninety Nine");
    }

    [Fact]
    public void FormatIndianCurrency_120000_FormatsWithCommas()
    {
        NumberToWordsConverter.FormatIndianCurrency(120000m).Should().Be("1,20,000");
    }

    [Fact]
    public void FormatIndianCurrency_Zero_ReturnsZero()
    {
        NumberToWordsConverter.FormatIndianCurrency(0).Should().Be("0");
    }
}
