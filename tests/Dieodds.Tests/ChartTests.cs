using System.Globalization;
using System.Numerics;

namespace Dieodds.Tests;

public class ChartTests
{
    private const string FortyHashes =
        "##########" + "##########" + "##########" + "##########";

    private const string EightyHashes =
        "##########" + "##########" + "##########" + "##########" +
        "##########" + "##########" + "##########" + "##########";

    [Fact]
    public void LinesRendersWeightedCountsAsAnExactAlignedTable()
    {
        var distribution = Dist.FromCounts([Entry(1, 1), Entry(2, 3)]);

        Assert.Equal(
            new[]
            {
                "value  chance  bar",
                "1      25.00%  #",
                "2      75.00%  ####"
            },
            Chart.Lines(distribution, 4));
    }

    [Fact]
    public void WidthOneStillDrawsOneHashForEveryPositiveOutcome()
    {
        var distribution = Dist.FromCounts([Entry(1, 1), Entry(2, 3)]);

        Assert.Equal(
            new[]
            {
                "value  chance  bar",
                "1      25.00%  #",
                "2      75.00%  #"
            },
            Chart.Lines(distribution, 1));
    }

    [Fact]
    public void LinesUsesMinimumBarLengthWhenIntegerScalingTruncatesToZero()
    {
        var distribution = Dist.FromCounts([Entry(1, 1), Entry(2, 5)]);

        Assert.Equal(
            new[]
            {
                "value  chance  bar",
                "1      16.67%  #",
                "2      83.33%  ####"
            },
            Chart.Lines(distribution, 4));
    }

    [Fact]
    public void LinesSortsNegativeValuesAndOmitsZeroWeights()
    {
        var distribution = Dist.FromCounts(
        [
            Entry(4, 2),
            Entry(8, 0),
            Entry(0, 1),
            Entry(-2, 2)
        ]);

        Assert.Equal(
            new[]
            {
                "value  chance  bar",
                "-2     40.00%  ######",
                "0      20.00%  ###",
                "4      40.00%  ######"
            },
            Chart.Lines(distribution, 6));
    }

    [Fact]
    public void LinesRendersConstantDistributionAtMaximumWidth()
    {
        var distribution = Dist.Constant(-3);

        Assert.Equal(
            new[]
            {
                "value  chance   bar",
                "-3     100.00%  " + EightyHashes
            },
            Chart.Lines(distribution, 80));
    }

    [Fact]
    public void LinesKeepsTinyPositiveSharesWithMinimumBars()
    {
        var distribution = Dist.FromCounts([Entry(0, 1), Entry(1, 99999)]);

        Assert.Equal(
            new[]
            {
                "value  chance   bar",
                "0      0.00%    #",
                "1      100.00%  " + FortyHashes
            },
            Chart.Lines(distribution, 40));
    }

    [Fact]
    public void LinesScalesCountsLargerThanInt64WithBigIntegerArithmetic()
    {
        var distribution = Dist.FromCounts(
        [
            Entry(-1, Big("1000000000000000000000000000000")),
            Entry(2, Big("2000000000000000000000000000000"))
        ]);

        Assert.Equal(
            new[]
            {
                "value  chance  bar",
                "-1     33.33%  " + FortyHashes,
                "2      66.67%  " + EightyHashes
            },
            Chart.Lines(distribution, 80));
    }

    [Fact]
    public void LinesUseInvariantFormattingUnderInvariantAndFrenchCultures()
    {
        var distribution = Dist.FromCounts([Entry(-1234, 1), Entry(5678, 1)]);
        var expected = new[]
        {
            "value  chance  bar",
            "-1234  50.00%  ####",
            "5678   50.00%  ####"
        };
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            Assert.Equal(expected, Chart.Lines(distribution, 4));

            var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentCulture = frenchCulture;
            CultureInfo.CurrentUICulture = frenchCulture;
            Assert.Equal(expected, Chart.Lines(distribution, 4));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void LinesHaveNoLineEndingsOrTrailingSpaces()
    {
        var distribution = Dist.FromCounts([Entry(-1, 2), Entry(3, 1)]);
        var lines = Chart.Lines(distribution, 4);

        foreach (var line in lines)
        {
            Assert.DoesNotContain('\n', line);
            Assert.DoesNotContain('\r', line);
            Assert.Equal(line.TrimEnd(), line);
        }
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
