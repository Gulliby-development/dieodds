using System.Globalization;
using System.Numerics;

namespace Dieodds.Tests;

public class ChanceTests
{
    [Theory]
    [InlineData("ge", Cmp.Ge)]
    [InlineData("gt", Cmp.Gt)]
    [InlineData("le", Cmp.Le)]
    [InlineData("lt", Cmp.Lt)]
    [InlineData("eq", Cmp.Eq)]
    [InlineData("ne", Cmp.Ne)]
    public void TryParseMapsEverySupportedToken(string text, Cmp expected)
    {
        Assert.True(Chance.TryParse(text, out var actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("GE")]
    [InlineData(" ge")]
    [InlineData("ge ")]
    [InlineData("")]
    [InlineData(">=")]
    public void TryParseRejectsEveryUnsupportedToken(string text)
    {
        Assert.False(Chance.TryParse(text, out _));
    }

    [Theory]
    [InlineData(Cmp.Ge, 8)]
    [InlineData(Cmp.Gt, 5)]
    [InlineData(Cmp.Le, 5)]
    [InlineData(Cmp.Lt, 2)]
    [InlineData(Cmp.Eq, 3)]
    [InlineData(Cmp.Ne, 7)]
    public void CountReturnsExactWeightedComparisonAtZero(Cmp cmp, int expected)
    {
        var distribution = Dist.FromCounts([Entry(-2, 2), Entry(0, 3), Entry(3, 5)]);

        Assert.Equal(new BigInteger(expected), Chance.Count(distribution, cmp, 0));
    }

    [Theory]
    [InlineData(Cmp.Ge, -2, 10)]
    [InlineData(Cmp.Gt, -2, 8)]
    [InlineData(Cmp.Le, 3, 10)]
    [InlineData(Cmp.Lt, 3, 5)]
    [InlineData(Cmp.Eq, -2, 2)]
    [InlineData(Cmp.Ne, 3, 5)]
    public void CountIncludesOrExcludesSupportBoundariesAsRequested(Cmp cmp, int n, int expected)
    {
        var distribution = Dist.FromCounts([Entry(-2, 2), Entry(0, 3), Entry(3, 5)]);

        Assert.Equal(new BigInteger(expected), Chance.Count(distribution, cmp, n));
    }

    [Fact]
    public void CountHandlesTargetsOutsideTheSupport()
    {
        var distribution = Dist.FromCounts([Entry(-2, 2), Entry(0, 3), Entry(3, 5)]);

        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Ge, 4));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Gt, 3));
        Assert.Equal(new BigInteger(10), Chance.Count(distribution, Cmp.Le, 4));
        Assert.Equal(new BigInteger(10), Chance.Count(distribution, Cmp.Lt, 4));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Eq, 4));
        Assert.Equal(new BigInteger(10), Chance.Count(distribution, Cmp.Ne, 4));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Le, -3));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Lt, -2));
        Assert.Equal(new BigInteger(10), Chance.Count(distribution, Cmp.Ge, -3));
        Assert.Equal(new BigInteger(10), Chance.Count(distribution, Cmp.Gt, -3));
    }

    [Fact]
    public void CountSupportsChanceTargetEndpoints()
    {
        var distribution = Dist.FromCounts([Entry(-100000, 2), Entry(100000, 3)]);

        Assert.Equal(new BigInteger(5), Chance.Count(distribution, Cmp.Ge, -100000));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Lt, -100000));
        Assert.Equal(new BigInteger(2), Chance.Count(distribution, Cmp.Eq, -100000));
        Assert.Equal(new BigInteger(5), Chance.Count(distribution, Cmp.Le, 100000));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Gt, 100000));
        Assert.Equal(new BigInteger(3), Chance.Count(distribution, Cmp.Eq, 100000));
    }

    [Fact]
    public void CountComparesIntegerExtremeKeysWithoutOverflow()
    {
        var distribution = Dist.FromCounts(
        [
            Entry(int.MinValue, 2),
            Entry(int.MaxValue, 3)
        ]);

        Assert.Equal(new BigInteger(5), Chance.Count(distribution, Cmp.Ge, int.MinValue));
        Assert.Equal(new BigInteger(3), Chance.Count(distribution, Cmp.Gt, int.MinValue));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Lt, int.MinValue));
        Assert.Equal(new BigInteger(3), Chance.Count(distribution, Cmp.Eq, int.MaxValue));
        Assert.Equal(BigInteger.Zero, Chance.Count(distribution, Cmp.Gt, int.MaxValue));
        Assert.Equal(new BigInteger(2), Chance.Count(distribution, Cmp.Ne, int.MaxValue));
    }

    [Fact]
    public void CountPreservesWeightsAndSumsBeyondInt64()
    {
        var smallerWeight = Big("9223372036854775808");
        var largerWeight = Big("9223372036854775810");
        var distribution = Dist.FromCounts(
        [
            Entry(-1, smallerWeight),
            Entry(2, largerWeight)
        ]);

        Assert.Equal(smallerWeight, Chance.Count(distribution, Cmp.Eq, -1));
        Assert.Equal(largerWeight, Chance.Count(distribution, Cmp.Ne, -1));
        Assert.Equal(largerWeight, Chance.Count(distribution, Cmp.Ge, 2));
        Assert.Equal(largerWeight, Chance.Count(distribution, Cmp.Gt, -1));
        Assert.Equal(smallerWeight, Chance.Count(distribution, Cmp.Le, -1));
        Assert.Equal(smallerWeight, Chance.Count(distribution, Cmp.Lt, 2));
    }

    [Fact]
    public void LineMatchesExactChanceExamplesIncludingZeroAndTinyShares()
    {
        var small = Dist.FromCounts([Entry(0, 3), Entry(1, 1)]);
        var tiny = Dist.FromCounts([Entry(0, 99999), Entry(1, 1)]);

        Assert.Equal("P(x eq 1) = 25.00% (1/4)", Chance.Line("x", "eq", 1, small));
        Assert.Equal("P(x gt 1) = 0.00% (0/1)", Chance.Line("x", "gt", 1, small));
        Assert.Equal("P(x ge 0) = 100.00% (1/1)", Chance.Line("x", "ge", 0, small));
        Assert.Equal("P(x eq 1) = 0.00% (1/100000)", Chance.Line("x", "eq", 1, tiny));
    }

    [Theory]
    [InlineData("ge", 0, "P(rolls + bonus ge 0) = 80.00% (4/5)")]
    [InlineData("gt", 0, "P(rolls + bonus gt 0) = 50.00% (1/2)")]
    [InlineData("le", 0, "P(rolls + bonus le 0) = 50.00% (1/2)")]
    [InlineData("lt", 0, "P(rolls + bonus lt 0) = 20.00% (1/5)")]
    [InlineData("eq", 0, "P(rolls + bonus eq 0) = 30.00% (3/10)")]
    [InlineData("ne", 0, "P(rolls + bonus ne 0) = 70.00% (7/10)")]
    public void LineFormatsEveryComparisonWithLiteralExpectedText(string op, int n, string expected)
    {
        var distribution = Dist.FromCounts([Entry(-2, 2), Entry(0, 3), Entry(3, 5)]);

        Assert.Equal(expected, Chance.Line("rolls + bonus", op, n, distribution));
    }

    [Fact]
    public void LinePreservesExpressionAndFormatsNegativeTargets()
    {
        var distribution = Dist.FromCounts([Entry(-2, 2), Entry(0, 3), Entry(3, 5)]);

        Assert.Equal(
            "P((d6 + 2) * -3 eq -2) = 20.00% (1/5)",
            Chance.Line("(d6 + 2) * -3", "eq", -2, distribution));
        Assert.Equal(
            "P(expression with spaces ge -3) = 100.00% (1/1)",
            Chance.Line("expression with spaces", "ge", -3, distribution));
    }

    [Fact]
    public void LineFormatsIntExtremeTargetsInvariantly()
    {
        var distribution = Dist.Constant(0);

        Assert.Equal(
            "P(edge eq -2147483648) = 0.00% (0/1)",
            Chance.Line("edge", "eq", int.MinValue, distribution));
        Assert.Equal(
            "P(edge eq 2147483647) = 0.00% (0/1)",
            Chance.Line("edge", "eq", int.MaxValue, distribution));
    }

    [Fact]
    public void LineFormattingIsCultureInvariantAndHasNoLineEnding()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var distribution = Dist.FromCounts([Entry(-100000, 2), Entry(100000, 3)]);

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            var invariant = Chance.Line("3d6kh2", "ge", -100000, distribution);

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
            var localized = Chance.Line("3d6kh2", "ge", -100000, distribution);

            Assert.Equal("P(3d6kh2 ge -100000) = 100.00% (1/1)", invariant);
            Assert.Equal(invariant, localized);
            Assert.False(localized.Contains('\r'));
            Assert.False(localized.Contains('\n'));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static KeyValuePair<int, BigInteger> Entry(int value, int count)
    {
        return new KeyValuePair<int, BigInteger>(value, new BigInteger(count));
    }

    private static KeyValuePair<int, BigInteger> Entry(int value, BigInteger count)
    {
        return new KeyValuePair<int, BigInteger>(value, count);
    }

    private static BigInteger Big(string value)
    {
        return BigInteger.Parse(value, CultureInfo.InvariantCulture);
    }
}
