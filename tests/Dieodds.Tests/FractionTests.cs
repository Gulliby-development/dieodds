using System.Globalization;
using System.Numerics;

namespace Dieodds.Tests;

public class FractionTests
{
    [Theory]
    [InlineData("12", "18", "2", "3")]
    [InlineData("-12", "18", "-2", "3")]
    [InlineData("0", "37", "0", "1")]
    [InlineData("1000000000000000000000000000000", "2000000000000000000000000000000", "1", "2")]
    public void ReduceDividesByGreatestCommonDivisor(
        string numerator,
        string denominator,
        string expectedNumerator,
        string expectedDenominator)
    {
        var actual = Fraction.Reduce(Big(numerator), Big(denominator));

        Assert.Equal((Big(expectedNumerator), Big(expectedDenominator)), actual);
    }

    [Theory]
    [InlineData("6", "36", "1/6")]
    [InlineData("-6", "36", "-1/6")]
    [InlineData("0", "7", "0/1")]
    public void RatioReturnsReducedInvariantFraction(
        string numerator,
        string denominator,
        string expected)
    {
        Assert.Equal(expected, Fraction.Ratio(Big(numerator), Big(denominator)));
    }

    [Theory]
    [InlineData("1", "8", "12.50%")]
    [InlineData("1", "30000", "0.00%")]
    public void PercentUsesTwoDecimalPlaces(
        string numerator,
        string denominator,
        string expected)
    {
        Assert.Equal(expected, Fraction.Percent(Big(numerator), Big(denominator)));
    }

    [Theory]
    [InlineData("1", "8", 2, "0.13")]
    [InlineData("-1", "200", 2, "-0.01")]
    [InlineData("-1", "300", 2, "0.00")]
    [InlineData("0", "7", 2, "0.00")]
    [InlineData("1", "2", 0, "1")]
    [InlineData("-1", "2", 0, "-1")]
    [InlineData("1", "3", 1, "0.3")]
    [InlineData("1", "8", 3, "0.125")]
    [InlineData("1", "8", 4, "0.1250")]
    [InlineData("1", "8", 5, "0.12500")]
    [InlineData("1", "8", 6, "0.125000")]
    [InlineData("999", "1000", 2, "1.00")]
    public void DecimalRoundsHalfAwayFromZeroWithFixedPrecision(
        string numerator,
        string denominator,
        int places,
        string expected)
    {
        Assert.Equal(expected, Fraction.Decimal(Big(numerator), Big(denominator), places));
    }

    [Fact]
    public void FormattingIsInvariantUnderNonEnglishCurrentCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var nonEnglishCulture = CultureInfo.GetCultureInfo("fr-FR");

        try
        {
            CultureInfo.CurrentCulture = nonEnglishCulture;
            CultureInfo.CurrentUICulture = nonEnglishCulture;

            Assert.Equal("12.50%", Fraction.Percent(Big("1"), Big("8")));
            Assert.Equal("1000000/3", Fraction.Ratio(Big("1000000"), Big("3")));
            Assert.Equal("-0.01", Fraction.Decimal(Big("-1"), Big("200"), 2));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static BigInteger Big(string value)
    {
        return BigInteger.Parse(value, CultureInfo.InvariantCulture);
    }
}
