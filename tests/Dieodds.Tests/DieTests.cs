using System.Numerics;

namespace Dieodds.Tests;

public class DieTests
{
    [Fact]
    public void OneProducesPlainD4Faces()
    {
        var distribution = Die.One(4, Mod.None, 0);

        Assert.Equal(
            new[] { Entry(1, 1), Entry(2, 1), Entry(3, 1), Entry(4, 1) },
            distribution.Items);
        Assert.Equal(new BigInteger(4), distribution.Total);
    }

    [Fact]
    public void OneProducesFudgeFaces()
    {
        var distribution = Die.One(0, Mod.None, 0);

        Assert.Equal(
            new[] { Entry(-1, 1), Entry(0, 1), Entry(1, 1) },
            distribution.Items);
        Assert.Equal(new BigInteger(3), distribution.Total);
    }

    [Fact]
    public void OneCountsTheTwoExplosionsOfD2()
    {
        var distribution = Die.One(2, Mod.Explode, 0);

        Assert.Equal(
            new[] { Entry(1, 4), Entry(3, 2), Entry(5, 1), Entry(6, 1) },
            distribution.Items);
        Assert.Equal(new BigInteger(8), distribution.Total);
    }

    [Fact]
    public void OneRerollsFacesAtOrBelowTheThresholdOnce()
    {
        var distribution = Die.One(3, Mod.Reroll, 1);

        Assert.Equal(
            new[] { Entry(1, 1), Entry(2, 4), Entry(3, 4) },
            distribution.Items);
        Assert.Equal(new BigInteger(9), distribution.Total);
    }

    [Fact]
    public void OneSupportsTheLargestValidRerollThreshold()
    {
        var distribution = Die.One(4, Mod.Reroll, 3);

        Assert.Equal(
            new[] { Entry(1, 3), Entry(2, 3), Entry(3, 3), Entry(4, 7) },
            distribution.Items);
        Assert.Equal(new BigInteger(16), distribution.Total);
    }

    [Fact]
    public void OfKeepsTheHighestOfTwoD4()
    {
        var distribution = KeepSum.Of(Die.One(4, Mod.None, 0), 2, KeepKind.High, 1);

        Assert.Equal(
            new[] { Entry(1, 1), Entry(2, 3), Entry(3, 5), Entry(4, 7) },
            distribution.Items);
        Assert.Equal(new BigInteger(16), distribution.Total);
    }

    [Fact]
    public void OfKeepsTheLowestTwoOfThreeD2()
    {
        var distribution = KeepSum.Of(Die.One(2, Mod.None, 0), 3, KeepKind.Low, 2);

        Assert.Equal(
            new[] { Entry(2, 4), Entry(3, 3), Entry(4, 1) },
            distribution.Items);
        Assert.Equal(new BigInteger(8), distribution.Total);
    }

    [Fact]
    public void OfKeepsAllDiceWhenKeptEqualsCount()
    {
        var distribution = KeepSum.Of(Die.One(2, Mod.None, 0), 2, KeepKind.High, 2);

        Assert.Equal(
            new[] { Entry(2, 1), Entry(3, 2), Entry(4, 1) },
            distribution.Items);
        Assert.Equal(new BigInteger(4), distribution.Total);
    }

    [Fact]
    public void OfCountsWeightedRerollsWhenKeepingTheHighest()
    {
        var distribution = KeepSum.Of(Die.One(3, Mod.Reroll, 1), 2, KeepKind.High, 1);

        Assert.Equal(
            new[] { Entry(1, 1), Entry(2, 24), Entry(3, 56) },
            distribution.Items);
        Assert.Equal(new BigInteger(81), distribution.Total);
    }

    [Fact]
    public void OfCountsWeightedRerollsWhenKeepingTheLowest()
    {
        var distribution = KeepSum.Of(Die.One(3, Mod.Reroll, 1), 2, KeepKind.Low, 1);

        Assert.Equal(
            new[] { Entry(1, 17), Entry(2, 48), Entry(3, 16) },
            distribution.Items);
        Assert.Equal(new BigInteger(81), distribution.Total);
    }

    [Fact]
    public void OfSumsThreeFudgeDiceWithHandCountedWeights()
    {
        var distribution = KeepSum.Of(Die.One(0, Mod.None, 0), 3, KeepKind.None, 3);

        Assert.Equal(
            new[]
            {
                Entry(-3, 1), Entry(-2, 3), Entry(-1, 6), Entry(0, 7),
                Entry(1, 6), Entry(2, 3), Entry(3, 1)
            },
            distribution.Items);
        Assert.Equal(new BigInteger(27), distribution.Total);
    }

    [Fact]
    public void OfReturnsTheOnlyOutcomeForOneKeptConstantDie()
    {
        var distribution = KeepSum.Of(Dist.Constant(-7), 1, KeepKind.High, 1);

        Assert.Equal(new[] { Entry(-7, 1) }, distribution.Items);
        Assert.Equal(BigInteger.One, distribution.Total);
    }

    [Fact]
    public void OfSumsAllDiceWhenKeepIsNoneRegardlessOfKept()
    {
        var distribution = KeepSum.Of(Die.One(2, Mod.None, 0), 2, KeepKind.None, 0);

        Assert.Equal(
            new[] { Entry(2, 1), Entry(3, 2), Entry(4, 1) },
            distribution.Items);
        Assert.Equal(new BigInteger(4), distribution.Total);
    }

    [Fact]
    public void OfHandlesTheMaximumPlainDiceCountAndSides()
    {
        var distribution = KeepSum.Of(Die.One(100, Mod.None, 0), 20, KeepKind.None, 0);

        Assert.Equal(BigInteger.Parse("10000000000000000000000000000000000000000"), distribution.Total);
        Assert.Equal(20, distribution.Items[0].Key);
        Assert.Equal(BigInteger.One, distribution.Items[0].Value);
        Assert.Equal(2000, distribution.Items[distribution.Items.Count - 1].Key);
        Assert.Equal(BigInteger.One, distribution.Items[distribution.Items.Count - 1].Value);
    }

    [Fact]
    public void OfHandlesMaximumExplodingDiceWithHighestKeep()
    {
        var distribution = KeepSum.Of(Die.One(20, Mod.Explode, 0), 10, KeepKind.High, 1);

        Assert.Equal(BigInteger.Parse("1073741824000000000000000000000000000000"), distribution.Total);
        Assert.Equal(1, distribution.Items[0].Key);
        Assert.Equal(BigInteger.Parse("104857600000000000000000000"), distribution.Items[0].Value);
        Assert.Equal(60, distribution.Items[distribution.Items.Count - 1].Key);
        Assert.Equal(
            BigInteger.Parse("1341422556883198016675901437120079999"),
            distribution.Items[distribution.Items.Count - 1].Value);
    }

    private static KeyValuePair<int, BigInteger> Entry(int value, BigInteger count)
    {
        return new KeyValuePair<int, BigInteger>(value, count);
    }
}
