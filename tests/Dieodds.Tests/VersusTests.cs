using System.Globalization;
using System.Numerics;

namespace Dieodds.Tests;

public class VersusTests
{
    [Fact]
    public void CountsAndLinesUsePairwiseProductsForWeightedDistributions()
    {
        var a = Dist.FromCounts([Entry(0, 1), Entry(2, 2)]);
        var b = Dist.FromCounts([Entry(1, 3), Entry(2, 1)]);

        AssertCounts(a, b, "6", "2", "4", "12");
        AssertLines(
        [
            "A: A d6 + 2",
            "B: B d8",
            "A wins: 50.00% (1/2)",
            "tie: 16.67% (1/6)",
            "B wins: 33.33% (1/3)"
        ], Versus.Lines("A d6 + 2", "B d8", a, b));
    }

    [Fact]
    public void CountsAndLinesHandleNegativeAndZeroSupports()
    {
        var a = Dist.FromCounts([Entry(-3, 2), Entry(0, 3)]);
        var b = Dist.FromCounts([Entry(-3, 1), Entry(-1, 4)]);

        AssertCounts(a, b, "15", "2", "8", "25");
        AssertLines(
        [
            "A: negative A",
            "B: negative B",
            "A wins: 60.00% (3/5)",
            "tie: 8.00% (2/25)",
            "B wins: 32.00% (8/25)"
        ], Versus.Lines("negative A", "negative B", a, b));
    }

    [Fact]
    public void EqualConstantsProduceOnlyTies()
    {
        var a = Dist.Constant(5);
        var b = Dist.Constant(5);

        AssertCounts(a, b, "0", "1", "0", "1");
        AssertLines(
        [
            "A: five",
            "B: five",
            "A wins: 0.00% (0/1)",
            "tie: 100.00% (1/1)",
            "B wins: 0.00% (0/1)"
        ], Versus.Lines("five", "five", a, b));
    }

    [Fact]
    public void CertainWinsAndLossesKeepZeroLines()
    {
        var high = Dist.Constant(8);
        var low = Dist.Constant(-2);

        AssertCounts(high, low, "1", "0", "0", "1");
        AssertLines(
        [
            "A: high",
            "B: low",
            "A wins: 100.00% (1/1)",
            "tie: 0.00% (0/1)",
            "B wins: 0.00% (0/1)"
        ], Versus.Lines("high", "low", high, low));

        AssertCounts(low, high, "0", "0", "1", "1");
        AssertLines(
        [
            "A: low",
            "B: high",
            "A wins: 0.00% (0/1)",
            "tie: 0.00% (0/1)",
            "B wins: 100.00% (1/1)"
        ], Versus.Lines("low", "high", low, high));
    }

    [Fact]
    public void SwappingWeightedOperandsSwapsWinsAndLosses()
    {
        var a = Dist.FromCounts([Entry(0, 1), Entry(2, 2)]);
        var b = Dist.FromCounts([Entry(1, 3), Entry(2, 1)]);

        AssertCounts(b, a, "4", "2", "6", "12");
        AssertLines(
        [
            "A: original B",
            "B: original A",
            "A wins: 33.33% (1/3)",
            "tie: 16.67% (1/6)",
            "B wins: 50.00% (1/2)"
        ], Versus.Lines("original B", "original A", b, a));
    }

    [Fact]
    public void TinySharesRetainTheirNonzeroAndZeroRatios()
    {
        var a = Dist.FromCounts([Entry(0, 99999), Entry(1, 1)]);
        var b = Dist.Constant(0);

        AssertCounts(a, b, "1", "99999", "0", "100000");
        AssertLines(
        [
            "A: rare win",
            "B: zero",
            "A wins: 0.00% (1/100000)",
            "tie: 100.00% (99999/100000)",
            "B wins: 0.00% (0/1)"
        ], Versus.Lines("rare win", "zero", a, b));
    }

    [Fact]
    public void ExtremeIntegerKeysAreComparedWithoutSubtraction()
    {
        var maximum = Dist.Constant(int.MaxValue);
        var minimum = Dist.Constant(int.MinValue);

        AssertCounts(maximum, minimum, "1", "0", "0", "1");
        AssertLines(
        [
            "A: maximum",
            "B: minimum",
            "A wins: 100.00% (1/1)",
            "tie: 0.00% (0/1)",
            "B wins: 0.00% (0/1)"
        ], Versus.Lines("maximum", "minimum", maximum, minimum));

        AssertCounts(minimum, maximum, "0", "0", "1", "1");
    }

    [Fact]
    public void PairwiseCountsAndFormattingExceedInt64()
    {
        var a = Dist.FromCounts(
        [
            Entry(0, Big("4000000000")),
            Entry(1, Big("6000000000"))
        ]);
        var b = Dist.FromCounts(
        [
            Entry(-1, Big("5000000000")),
            Entry(0, Big("3000000000"))
        ]);

        AssertCounts(a, b, "68000000000000000000", "12000000000000000000", "0", "80000000000000000000");
        AssertLines(
        [
            "A: large A",
            "B: large B",
            "A wins: 85.00% (17/20)",
            "tie: 15.00% (3/20)",
            "B wins: 0.00% (0/1)"
        ], Versus.Lines("large A", "large B", a, b));
    }

    [Fact]
    public void LinesPreserveLabelsAndUseInvariantFormattingAcrossCultures()
    {
        var a = Dist.FromCounts([Entry(-1, 1), Entry(0, 2)]);
        var b = Dist.FromCounts([Entry(-1, 3), Entry(1, 1)]);
        string[] expected =
        [
            "A:  d6 + 1 ",
            "B: 2d6 ",
            "A wins: 50.00% (1/2)",
            "tie: 25.00% (1/4)",
            "B wins: 25.00% (1/4)"
        ];

        AssertCounts(a, b, "6", "3", "3", "12");

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            AssertLines(expected, Versus.Lines(" d6 + 1 ", "2d6 ", a, b));

            var nonEnglishCulture = CultureInfo.GetCultureInfo("fr-FR");
            CultureInfo.CurrentCulture = nonEnglishCulture;
            CultureInfo.CurrentUICulture = nonEnglishCulture;
            AssertLines(expected, Versus.Lines(" d6 + 1 ", "2d6 ", a, b));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static void AssertCounts(
        Dist a,
        Dist b,
        string expectedWin,
        string expectedTie,
        string expectedLose,
        string expectedTotal)
    {
        var actual = Versus.Counts(a, b);

        Assert.Equal((Big(expectedWin), Big(expectedTie), Big(expectedLose)), actual);
        Assert.Equal(Big(expectedTotal), a.Total * b.Total);
        Assert.Equal(Big(expectedTotal), actual.Win + actual.Tie + actual.Lose);
    }

    private static void AssertLines(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
    {
        Assert.Equal(expected, actual);
        Assert.All(actual, line =>
        {
            Assert.DoesNotContain("\r", line);
            Assert.DoesNotContain("\n", line);
        });
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
