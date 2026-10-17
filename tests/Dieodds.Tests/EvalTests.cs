using System.Numerics;

namespace Dieodds.Tests;

public class EvalTests
{
    [Fact]
    public void EvaluateNumberReturnsConstantDistribution()
    {
        AssertDistribution(new Number(2), [Entry(2, 1)], 1);
    }

    [Fact]
    public void EvaluateDiceUsesTheDieAndKeepEngines()
    {
        AssertDistribution(
            new Dice(1, 2, Mod.None, 0, KeepKind.None, 1),
            [Entry(1, 1), Entry(2, 1)],
            2);
    }

    [Fact]
    public void EvaluateGroupReturnsItsInnerDistribution()
    {
        AssertDistribution(new Group(new Number(-3)), [Entry(-3, 1)], 1);
    }

    [Fact]
    public void EvaluateNegatesValuesWithoutChangingCounts()
    {
        AssertDistribution(new Neg(new Number(2)), [Entry(-2, 1)], 1);
    }

    [Fact]
    public void EvaluateAdditionConvolvesIndependentDistributions()
    {
        AssertDistribution(
            new Binary('+', new Dice(1, 2, Mod.None, 0, KeepKind.None, 1), new Dice(1, 2, Mod.None, 0, KeepKind.None, 1)),
            [Entry(2, 1), Entry(3, 2), Entry(4, 1)],
            4);
    }

    [Fact]
    public void EvaluateSubtractionCountsLeftMinusRight()
    {
        AssertDistribution(
            new Binary('-', new Dice(1, 2, Mod.None, 0, KeepKind.None, 1), new Dice(1, 2, Mod.None, 0, KeepKind.None, 1)),
            [Entry(-1, 1), Entry(0, 2), Entry(1, 1)],
            4);
    }

    [Fact]
    public void EvaluateScalesByAConstantOnTheRight()
    {
        AssertDistribution(
            new Binary('*', new Dice(1, 2, Mod.None, 0, KeepKind.None, 1), new Number(2)),
            [Entry(2, 1), Entry(4, 1)],
            2);
    }

    [Fact]
    public void EvaluateScalesByAConstantOnTheLeft()
    {
        AssertDistribution(
            new Binary('*', new Number(2), new Dice(1, 2, Mod.None, 0, KeepKind.None, 1)),
            [Entry(2, 1), Entry(4, 1)],
            2);
    }

    [Fact]
    public void EvaluateMultiplicationOfTwoNumbersUsesTheRightNumber()
    {
        AssertDistribution(new Binary('*', new Number(2), new Number(3)), [Entry(6, 1)], 1);
    }

    [Fact]
    public void EvaluateParsedMultiplicationChainPreservesOutcomeTotal()
    {
        AssertParsedDistribution("d2*2*3", [Entry(6, 1), Entry(12, 1)], 2);
    }

    [Fact]
    public void EvaluateZeroScalingMergesValuesAndRetainsTheTotal()
    {
        AssertParsedDistribution("d2*0", [Entry(0, 2)], 2);
    }

    [Fact]
    public void EvaluateParsedGroupAndMultiplication()
    {
        AssertParsedDistribution("(d2+1)*3", [Entry(6, 1), Entry(9, 1)], 2);
    }

    [Fact]
    public void EvaluateParsedLeadingNegationCoversTheFirstTermOnly()
    {
        AssertParsedDistribution("-d2*2+1", [Entry(-3, 1), Entry(-1, 1)], 2);
    }

    [Fact]
    public void EvaluateExplodingDiceKeepsExactWeights()
    {
        AssertDistribution(
            new Dice(1, 2, Mod.Explode, 0, KeepKind.None, 1),
            [Entry(1, 4), Entry(3, 2), Entry(5, 1), Entry(6, 1)],
            8);
    }

    [Fact]
    public void EvaluateRerollDiceKeepsExactWeights()
    {
        AssertDistribution(
            new Dice(1, 3, Mod.Reroll, 1, KeepKind.None, 1),
            [Entry(1, 1), Entry(2, 4), Entry(3, 4)],
            9);
    }

    [Fact]
    public void EvaluateHighestKeepCountsAllLabeledSequences()
    {
        AssertDistribution(
            new Dice(2, 4, Mod.None, 0, KeepKind.High, 1),
            [Entry(1, 1), Entry(2, 3), Entry(3, 5), Entry(4, 7)],
            16);
    }

    [Fact]
    public void EvaluateLowestKeepCountsAllLabeledSequences()
    {
        AssertDistribution(
            new Dice(3, 2, Mod.None, 0, KeepKind.Low, 2),
            [Entry(2, 4), Entry(3, 3), Entry(4, 1)],
            8);
    }

    [Fact]
    public void EvaluateNoKeepSumsEveryDie()
    {
        AssertDistribution(
            new Dice(3, 2, Mod.None, 0, KeepKind.None, 3),
            [Entry(3, 1), Entry(4, 3), Entry(5, 3), Entry(6, 1)],
            8);
    }

    [Fact]
    public void EvaluateParsedSumAndDifferenceMatchHandCountedDistributions()
    {
        AssertParsedDistribution("d2+d2", [Entry(2, 1), Entry(3, 2), Entry(4, 1)], 4);
        AssertParsedDistribution("d2-d2", [Entry(-1, 1), Entry(0, 2), Entry(1, 1)], 4);
    }

    private static void AssertParsedDistribution(
        string expression,
        KeyValuePair<int, BigInteger>[] expected,
        BigInteger total)
    {
        AssertDistribution(Parser.Parse(expression), expected, total);
    }

    private static void AssertDistribution(
        Node node,
        KeyValuePair<int, BigInteger>[] expected,
        BigInteger total)
    {
        var distribution = Eval.Evaluate(node);

        Assert.Equal(expected, distribution.Items);
        Assert.Equal(total, distribution.Total);
    }

    private static KeyValuePair<int, BigInteger> Entry(int value, BigInteger count)
    {
        return new KeyValuePair<int, BigInteger>(value, count);
    }
}
