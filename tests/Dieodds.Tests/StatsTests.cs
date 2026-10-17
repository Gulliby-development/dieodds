using System.Globalization;
using System.Numerics;

namespace Dieodds.Tests;

public class StatsTests
{
    [Fact]
    public void WeightedDistributionMatchesEverySummaryLine()
    {
        var distribution = Dist.FromCounts([Entry(-1, 1), Entry(1, 3)]);

        AssertLines(
            [
                "expression: x",
                "outcomes: 4",
                "min: -1",
                "max: 1",
                "mean: 0.50",
                "variance: 0.75",
                "median: 1",
                "mode: 1"
            ],
            Stats.Lines("x", distribution));
    }

    [Fact]
    public void UniformFourValuesListsEveryTiedMode()
    {
        var distribution = Dist.FromCounts([Entry(1, 1), Entry(2, 1), Entry(3, 1), Entry(4, 1)]);

        AssertLines(
            [
                "expression: d4",
                "outcomes: 4",
                "min: 1",
                "max: 4",
                "mean: 2.50",
                "variance: 1.25",
                "median: 2",
                "mode: 1, 2, 3, 4"
            ],
            Stats.Lines("d4", distribution));
    }

    [Fact]
    public void ConstantNegativeDistributionHasZeroVariance()
    {
        AssertLines(
            [
                "expression: -3",
                "outcomes: 1",
                "min: -3",
                "max: -3",
                "mean: -3.00",
                "variance: 0.00",
                "median: -3",
                "mode: -3"
            ],
            Stats.Lines("-3", Dist.Constant(-3)));
    }

    [Fact]
    public void WeightedNegativeValuesUseExactMomentsAndAscendingSupport()
    {
        var distribution = Dist.FromCounts([Entry(-5, 2), Entry(-1, 1), Entry(2, 3)]);

        AssertLines(
            [
                "expression: weighted",
                "outcomes: 6",
                "min: -5",
                "max: 2",
                "mean: -0.83",
                "variance: 9.81",
                "median: -1",
                "mode: 2"
            ],
            Stats.Lines("weighted", distribution));
    }

    [Fact]
    public void MedianSelectsLowerValueAtAnExactHalf()
    {
        var distribution = Dist.FromCounts([Entry(-4, 1), Entry(2, 1)]);

        AssertLines(
            [
                "expression: split",
                "outcomes: 2",
                "min: -4",
                "max: 2",
                "mean: -1.00",
                "variance: 9.00",
                "median: -4",
                "mode: -4, 2"
            ],
            Stats.Lines("split", distribution));
    }

    [Fact]
    public void OutcomesLinePreservesTheUnreducedCountTotal()
    {
        var distribution = Dist.FromCounts([Entry(1, 2), Entry(3, 4)]);

        AssertLines(
            [
                "expression: weighted",
                "outcomes: 6",
                "min: 1",
                "max: 3",
                "mean: 2.33",
                "variance: 0.89",
                "median: 3",
                "mode: 3"
            ],
            Stats.Lines("weighted", distribution));
    }

    [Fact]
    public void MeanRoundsPositiveAndNegativeHalfAwayFromZero()
    {
        var positive = Dist.FromCounts([Entry(1, 7), Entry(2, 1)]);
        var negative = Dist.FromCounts([Entry(-2, 1), Entry(-1, 7)]);

        AssertLines(
            [
                "expression: positive",
                "outcomes: 8",
                "min: 1",
                "max: 2",
                "mean: 1.13",
                "variance: 0.11",
                "median: 1",
                "mode: 1"
            ],
            Stats.Lines("positive", positive));
        AssertLines(
            [
                "expression: negative",
                "outcomes: 8",
                "min: -2",
                "max: -1",
                "mean: -1.13",
                "variance: 0.11",
                "median: -1",
                "mode: -1"
            ],
            Stats.Lines("negative", negative));
    }

    [Fact]
    public void SquaringValuesBeyondInt32UsesBigInteger()
    {
        var distribution = Dist.FromCounts([Entry(-50000, 1), Entry(50000, 1)]);

        AssertLines(
            [
                "expression: large-squares",
                "outcomes: 2",
                "min: -50000",
                "max: 50000",
                "mean: 0.00",
                "variance: 2500000000.00",
                "median: -50000",
                "mode: -50000, 50000"
            ],
            Stats.Lines("large-squares", distribution));
    }

    [Fact]
    public void CountsAndMomentsCanExceedInt64()
    {
        var largeCount = Big("1000000000000000000000000000000");
        var distribution = Dist.FromCounts([Entry(1, largeCount), Entry(2, largeCount)]);

        AssertLines(
            [
                "expression: huge",
                "outcomes: 2000000000000000000000000000000",
                "min: 1",
                "max: 2",
                "mean: 1.50",
                "variance: 0.25",
                "median: 1",
                "mode: 1, 2"
            ],
            Stats.Lines("huge", distribution));
    }

    [Fact]
    public void ExpressionLabelIsPreservedVerbatim()
    {
        var label = " 2d6 + 1 ";

        AssertLines(
            [
                "expression:  2d6 + 1 ",
                "outcomes: 1",
                "min: 3",
                "max: 3",
                "mean: 3.00",
                "variance: 0.00",
                "median: 3",
                "mode: 3"
            ],
            Stats.Lines(label, Dist.Constant(3)));
    }

    [Fact]
    public void OutputUsesInvariantFormattingAcrossCulturesAndHasNoLineEndings()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        var distribution = Dist.FromCounts([Entry(-1, 1), Entry(1, 3)]);
        var expected = new[]
        {
            "expression: x",
            "outcomes: 4",
            "min: -1",
            "max: 1",
            "mean: 0.50",
            "variance: 0.75",
            "median: 1",
            "mode: 1"
        };

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            AssertLines(expected, Stats.Lines("x", distribution));

            var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentCulture = frenchCulture;
            CultureInfo.CurrentUICulture = frenchCulture;
            AssertLines(expected, Stats.Lines("x", distribution));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static void AssertLines(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
    {
        Assert.Equal(expected, actual);
        Assert.All(actual, line =>
        {
            Assert.DoesNotContain("\n", line);
            Assert.DoesNotContain("\r", line);
        });
    }

    private static KeyValuePair<int, BigInteger> Entry(int value, int count)
    {
        return Entry(value, new BigInteger(count));
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
