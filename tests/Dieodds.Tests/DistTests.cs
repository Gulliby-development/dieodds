using System.Numerics;

namespace Dieodds.Tests;

public class DistTests
{
    [Fact]
    public void FromCountsMergesDuplicatesDropsZerosAndSortsByValue()
    {
        var distribution = Dist.FromCounts(
        [
            Entry(2, 3),
            Entry(-1, 2),
            Entry(2, 4),
            Entry(9, 0)
        ]);

        Assert.Equal(
            new[] { Entry(-1, 2), Entry(2, 7) },
            distribution.Items);
        Assert.Equal(new BigInteger(9), distribution.Total);
    }

    [Fact]
    public void FromCountsPreservesCountsLargerThanPrimitiveIntegers()
    {
        var largeCount = BigInteger.Parse("1000000000000000000000000000000");
        var expectedTotal = BigInteger.Parse("1000000000000000000000000000001");
        var distribution = Dist.FromCounts(
        [
            Entry(1, largeCount),
            Entry(2, BigInteger.One)
        ]);

        Assert.Equal(
            new[] { Entry(1, largeCount), Entry(2, BigInteger.One) },
            distribution.Items);
        Assert.Equal(expectedTotal, distribution.Total);
    }

    [Fact]
    public void ConstantCreatesOneOutcomeWithCountOne()
    {
        var distribution = Dist.Constant(-3);

        Assert.Equal(new[] { Entry(-3, 1) }, distribution.Items);
        Assert.Equal(BigInteger.One, distribution.Total);
    }

    [Fact]
    public void AddConvolvesEveryPairAndPreservesTheProductTotal()
    {
        var left = Dist.FromCounts([Entry(1, 2), Entry(2, 1)]);
        var right = Dist.FromCounts([Entry(0, 1), Entry(1, 3)]);

        var sum = left.Add(right);

        Assert.Equal(
            new[] { Entry(1, 2), Entry(2, 7), Entry(3, 3) },
            sum.Items);
        Assert.Equal(new BigInteger(12), sum.Total);
        Assert.Equal(new[] { Entry(1, 2), Entry(2, 1) }, left.Items);
        Assert.Equal(new[] { Entry(0, 1), Entry(1, 3) }, right.Items);
    }

    [Fact]
    public void SubtractConvolvesLeftMinusRightAndPreservesTheProductTotal()
    {
        var left = Dist.FromCounts([Entry(1, 2), Entry(2, 1)]);
        var right = Dist.FromCounts([Entry(0, 1), Entry(1, 3)]);

        var difference = left.Subtract(right);

        Assert.Equal(
            new[] { Entry(0, 6), Entry(1, 5), Entry(2, 1) },
            difference.Items);
        Assert.Equal(new BigInteger(12), difference.Total);
        Assert.Equal(new[] { Entry(1, 2), Entry(2, 1) }, left.Items);
        Assert.Equal(new[] { Entry(0, 1), Entry(1, 3) }, right.Items);
    }

    [Fact]
    public void NegateReversesValuesAndRetainsCountsAndTotal()
    {
        var distribution = Dist.FromCounts([Entry(1, 2), Entry(2, 1)]);

        var negated = distribution.Negate();

        Assert.Equal(new[] { Entry(-2, 1), Entry(-1, 2) }, negated.Items);
        Assert.Equal(new BigInteger(3), negated.Total);
        Assert.Equal(new[] { Entry(1, 2), Entry(2, 1) }, distribution.Items);
    }

    [Fact]
    public void ScaleByNegativeFactorReversesAndMergesValuesInAscendingOrder()
    {
        var distribution = Dist.FromCounts([Entry(1, 2), Entry(2, 1)]);

        var scaled = distribution.Scale(-2);

        Assert.Equal(new[] { Entry(-4, 1), Entry(-2, 2) }, scaled.Items);
        Assert.Equal(new BigInteger(3), scaled.Total);
        Assert.Equal(new[] { Entry(1, 2), Entry(2, 1) }, distribution.Items);
    }

    [Fact]
    public void ScaleByZeroMergesAllValuesWithoutChangingTotal()
    {
        var distribution = Dist.FromCounts([Entry(1, 2), Entry(2, 1)]);

        var scaled = distribution.Scale(0);

        Assert.Equal(new[] { Entry(0, 3) }, scaled.Items);
        Assert.Equal(new BigInteger(3), scaled.Total);
        Assert.Equal(new[] { Entry(1, 2), Entry(2, 1) }, distribution.Items);
    }

    [Fact]
    public void ScaleByPositiveFactorRetainsCountsAndTotal()
    {
        var distribution = Dist.FromCounts([Entry(1, 2), Entry(2, 1)]);

        var scaled = distribution.Scale(2);

        Assert.Equal(new[] { Entry(2, 2), Entry(4, 1) }, scaled.Items);
        Assert.Equal(new BigInteger(3), scaled.Total);
        Assert.Equal(new[] { Entry(1, 2), Entry(2, 1) }, distribution.Items);
    }

    [Fact]
    public void AddMergesPairsThatProduceTheSameValue()
    {
        var left = Dist.FromCounts([Entry(0, 2)]);
        var right = Dist.FromCounts([Entry(0, 3)]);

        var sum = left.Add(right);

        Assert.Equal(new[] { Entry(0, 6) }, sum.Items);
        Assert.Equal(new BigInteger(6), sum.Total);
    }

    [Fact]
    public void FromCountsOwnsItsSourceCollection()
    {
        var source = new List<KeyValuePair<int, BigInteger>>
        {
            Entry(2, 3),
            Entry(-1, 2)
        };
        var distribution = Dist.FromCounts(source);

        source[0] = Entry(8, 20);
        source.Add(Entry(5, 1));

        Assert.Equal(new[] { Entry(-1, 2), Entry(2, 3) }, distribution.Items);
        Assert.Equal(new BigInteger(5), distribution.Total);
    }

    [Fact]
    public void ItemsCannotBeUsedToMutateTheDistribution()
    {
        var distribution = Dist.FromCounts([Entry(1, 2), Entry(2, 1)]);
        var items = Assert.IsAssignableFrom<IList<KeyValuePair<int, BigInteger>>>(distribution.Items);

        Assert.True(items.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => items[0] = Entry(9, 9));
        Assert.Equal(new[] { Entry(1, 2), Entry(2, 1) }, distribution.Items);
        Assert.Equal(new BigInteger(3), distribution.Total);
    }

    private static KeyValuePair<int, BigInteger> Entry(int value, BigInteger count)
    {
        return new KeyValuePair<int, BigInteger>(value, count);
    }
}
