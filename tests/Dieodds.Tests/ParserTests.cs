namespace Dieodds.Tests;

public class ParserTests
{
    [Fact]
    public void ParsesZeroNumber()
    {
        Assert.Equal(new Number(0), Parser.Parse("0"));
    }

    [Fact]
    public void ParsesMaximumFourDigitNumber()
    {
        Assert.Equal(new Number(9999), Parser.Parse("9999"));
    }

    [Fact]
    public void RejectsLeadingZeroNumber()
    {
        AssertError("007", "invalid number \"007\" at column 1");
    }

    [Fact]
    public void RejectsFiveDigitNumber()
    {
        AssertError("10000", "invalid number \"10000\" at column 1");
    }

    [Fact]
    public void PreservesParenthesizedNumberAsGroup()
    {
        Assert.Equal(new Group(new Number(2)), Parser.Parse("(2)"));
    }

    [Fact]
    public void PreservesNestedGroups()
    {
        Assert.Equal(new Group(new Group(new Number(2))), Parser.Parse("((2))"));
    }

    [Fact]
    public void MultiplicationBindsMoreTightlyThanAddition()
    {
        Assert.Equal(
            new Binary('+', new Number(2), new Binary('*', new Number(3), new Number(4))),
            Parser.Parse("2+3*4"));
    }

    [Fact]
    public void AdditionAssociatesLeftToRight()
    {
        Assert.Equal(
            new Binary('+', new Binary('+', new Number(1), new Number(2)), new Number(3)),
            Parser.Parse("1+2+3"));
    }

    [Fact]
    public void SubtractionAssociatesLeftToRight()
    {
        Assert.Equal(
            new Binary('-', new Binary('-', new Number(9), new Number(4)), new Number(2)),
            Parser.Parse("9-4-2"));
    }

    [Fact]
    public void LeadingMinusNegatesTheWholeFirstTerm()
    {
        Assert.Equal(
            new Binary(
                '+',
                new Neg(new Binary('*', new Dice(1, 6, Mod.None, 0, KeepKind.None, 1), new Number(2))),
                new Number(1)),
            Parser.Parse("-d6*2+1"));
    }

    [Fact]
    public void LeadingMinusDoesNotNegateLaterTerms()
    {
        Assert.Equal(
            new Binary('-', new Neg(new Number(2)), new Number(3)),
            Parser.Parse("-2-3"));
    }

    [Fact]
    public void SpacesAndTabsSeparateExpressionItems()
    {
        Assert.Equal(
            new Binary('+', new Number(2), new Binary('*', new Number(3), new Number(4))),
            Parser.Parse("2 \t+\t 3 * 4"));
    }

    [Fact]
    public void ParsesPlainDieWithNeutralDefaults()
    {
        Assert.Equal(new Dice(1, 6, Mod.None, 0, KeepKind.None, 1), Parser.Parse("d6"));
    }

    [Fact]
    public void ParsesPercentDieAsOneHundredSides()
    {
        Assert.Equal(new Dice(1, 100, Mod.None, 0, KeepKind.None, 1), Parser.Parse("d%"));
    }

    [Fact]
    public void ParsesFudgeDieWithZeroSidesSentinel()
    {
        Assert.Equal(new Dice(1, 0, Mod.None, 0, KeepKind.None, 1), Parser.Parse("dF"));
    }

    [Fact]
    public void ParsesCountAndKeepsAllDiceWhenKeepIsAbsent()
    {
        Assert.Equal(new Dice(4, 6, Mod.None, 0, KeepKind.None, 4), Parser.Parse("4d6"));
    }

    [Fact]
    public void ParsesHighKeepDefaultOfOne()
    {
        Assert.Equal(new Dice(4, 6, Mod.None, 0, KeepKind.High, 1), Parser.Parse("4d6kh"));
    }

    [Fact]
    public void ParsesLowKeepWithExplicitCount()
    {
        Assert.Equal(new Dice(4, 6, Mod.None, 0, KeepKind.Low, 2), Parser.Parse("4d6kl2"));
    }

    [Fact]
    public void ParsesRerollBeforeKeep()
    {
        Assert.Equal(new Dice(2, 6, Mod.Reroll, 1, KeepKind.High, 1), Parser.Parse("2d6r1kh"));
    }

    [Fact]
    public void ParsesExplosionBeforeLowKeep()
    {
        Assert.Equal(new Dice(2, 6, Mod.Explode, 0, KeepKind.Low, 1), Parser.Parse("2d6!kl"));
    }

    [Fact]
    public void ParsesNumberOnLeftOfMultiplication()
    {
        Assert.Equal(
            new Binary('*', new Number(2), new Dice(1, 6, Mod.None, 0, KeepKind.None, 1)),
            Parser.Parse("2*d6"));
    }

    [Fact]
    public void ParsesNumberOnRightOfMultiplication()
    {
        Assert.Equal(
            new Binary('*', new Dice(1, 6, Mod.None, 0, KeepKind.None, 1), new Number(2)),
            Parser.Parse("d6*2"));
    }

    [Fact]
    public void ParsesChainWithRightNumberOnEveryMultiplication()
    {
        Assert.Equal(
            new Binary(
                '*',
                new Binary('*', new Dice(1, 6, Mod.None, 0, KeepKind.None, 1), new Number(2)),
                new Number(3)),
            Parser.Parse("d6*2*3"));
    }

    [Fact]
    public void ParsesMultiplicationBetweenTwoNumbers()
    {
        Assert.Equal(new Binary('*', new Number(2), new Number(3)), Parser.Parse("2*3"));
    }

    [Fact]
    public void ParsesAtMostThreeDiceTermsAcrossGroups()
    {
        Assert.Equal(
            new Binary('+',
                new Binary('+',
                    new Group(new Dice(1, 2, Mod.None, 0, KeepKind.None, 1)),
                    new Group(new Dice(1, 2, Mod.None, 0, KeepKind.None, 1))),
                new Group(new Dice(1, 2, Mod.None, 0, KeepKind.None, 1))),
            Parser.Parse("(d2)+(d2)+(d2)"));
    }

    [Fact]
    public void AcceptsPlainDiceAtBothCountAndSideBoundaries()
    {
        Assert.Equal(new Dice(1, 2, Mod.None, 0, KeepKind.None, 1), Parser.Parse("d2"));
        Assert.Equal(new Dice(20, 100, Mod.None, 0, KeepKind.None, 20), Parser.Parse("20d100"));
    }

    [Fact]
    public void AcceptsModifiedDiceAtCountAndSideBoundaries()
    {
        Assert.Equal(new Dice(10, 20, Mod.Explode, 0, KeepKind.High, 10), Parser.Parse("10d20!kh10"));
    }

    [Fact]
    public void AcceptsMinimumRerollAndMaximumReroll()
    {
        Assert.Equal(new Dice(1, 2, Mod.Reroll, 1, KeepKind.None, 1), Parser.Parse("d2r1"));
        Assert.Equal(new Dice(1, 20, Mod.Reroll, 19, KeepKind.None, 1), Parser.Parse("d20r19"));
    }

    [Fact]
    public void RejectsEmptyExpression()
    {
        AssertError("", "empty expression");
    }

    [Fact]
    public void RejectsExpressionLongerThanTwoHundredCharactersBeforeParsing()
    {
        const string maximumLengthExpression =
            "11*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1" +
            "*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1" +
            "*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1" +
            "*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1" +
            "*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1*1";
        const string tooLongExpression = maximumLengthExpression + "@";

        Assert.Equal(200, maximumLengthExpression.Length);
        Assert.IsType<Binary>(Parser.Parse(maximumLengthExpression));
        Assert.Equal(201, tooLongExpression.Length);
        AssertError(tooLongExpression, "expression is longer than 200 characters");
    }

    [Fact]
    public void ReportsUnexpectedUppercaseCharacter()
    {
        AssertError("X6", "unexpected \"X\" at column 1");
    }

    [Fact]
    public void ReportsUnexpectedSecondSign()
    {
        AssertError("1+-2", "unexpected \"-\" at column 3");
    }

    [Fact]
    public void ReportsUnexpectedClosingParenthesis()
    {
        AssertError("1)", "unexpected \")\" at column 2");
    }

    [Fact]
    public void ReportsUnexpectedUnknownCharacter()
    {
        AssertError("@", "unexpected \"@\" at column 1");
    }

    [Fact]
    public void ReportsUnexpectedEndAfterBinaryOperator()
    {
        AssertError("2d6+", "unexpected end of the expression");
    }

    [Fact]
    public void ReportsUnexpectedEndForUnclosedGroup()
    {
        AssertError("(d6", "unexpected end of the expression");
    }

    [Fact]
    public void ReportsUnexpectedEndWhenDieSidesAreMissing()
    {
        AssertError("d", "unexpected end of the expression");
    }

    [Fact]
    public void ReportsUnexpectedCharacterWhenDieSidesAreMalformed()
    {
        AssertError("d?", "unexpected \"?\" at column 2");
    }

    [Fact]
    public void ReportsUnexpectedEndWhenRerollDigitsAreMissing()
    {
        AssertError("d6r", "unexpected end of the expression");
    }

    [Fact]
    public void ReportsUnexpectedCharacterWhenRerollDigitsAreMalformed()
    {
        AssertError("d6rX", "unexpected \"X\" at column 4");
    }

    [Fact]
    public void ReportsUnexpectedEndWhenKeepKindIsMissing()
    {
        AssertError("d6k", "unexpected end of the expression");
    }

    [Fact]
    public void ReportsUnexpectedCharacterWhenKeepKindIsMalformed()
    {
        AssertError("d6kx", "unexpected \"x\" at column 4");
    }

    [Fact]
    public void ReportsUnexpectedCharacterForWhitespaceInsideNumber()
    {
        AssertError("1 2", "unexpected \"2\" at column 3");
    }

    [Fact]
    public void ReportsUnexpectedCharacterForWhitespaceInsideDie()
    {
        AssertError("d 6", "unexpected \"?\" at column 2");
    }

    [Fact]
    public void ReportsUnexpectedCharacterForWhitespaceBeforeModifier()
    {
        AssertError("d6 r1", "unexpected \"r\" at column 4");
    }

    [Fact]
    public void ReportsOnlyOneUtf16UnitOfAnAstralUnexpectedCharacter()
    {
        AssertError("😀", "unexpected \"?\" at column 1");
    }

    [Fact]
    public void SanitizesNewlineAndUsesItsUtf16Column()
    {
        AssertError("d6\n+2", "unexpected \"?\" at column 3");
    }

    [Fact]
    public void ReportsSyntaxErrorForSecondModifierBeforeKeep()
    {
        AssertError("2d6!r1", "unexpected \"r\" at column 5");
    }

    [Fact]
    public void ReportsSyntaxErrorForSecondModifier()
    {
        AssertError("d6!!", "unexpected \"!\" at column 4");
    }

    [Fact]
    public void ReportsSyntaxErrorForModifierAfterKeep()
    {
        AssertError("d6kh1r1", "unexpected \"r\" at column 6");
    }

    [Fact]
    public void ReadsRightFactorErrorBeforeMultiplicationRestriction()
    {
        AssertError("d6*21d6", "invalid dice \"21d6\" at column 4 (1 to 20 dice)");
    }

    [Fact]
    public void RequiresNumberOnOneSideOfMultiplication()
    {
        AssertError("d6*d6", "* needs a number on one side at column 3");
    }

    [Fact]
    public void GroupedNumberDoesNotSatisfyMultiplicationRequirement()
    {
        AssertError("(2)*d6", "* needs a number on one side at column 4");
    }

    [Fact]
    public void ChainedMultiplicationChecksEachStar()
    {
        AssertError("d6*2*d4", "* needs a number on one side at column 5");
    }

    [Fact]
    public void MissingRightFactorIsReportedBeforeMultiplicationRestriction()
    {
        AssertError("d6*", "unexpected end of the expression");
    }

    [Fact]
    public void RejectsZeroDiceBeforeInvalidSides()
    {
        AssertError("0d1", "invalid dice \"0d1\" at column 1 (1 to 20 dice)");
    }

    [Fact]
    public void ModifiedDiceHaveTheLowerDiceCountLimit()
    {
        AssertError("11d6kh1", "invalid dice \"11d6kh1\" at column 1 (1 to 10 dice with a modifier)");
    }

    [Fact]
    public void RejectsInvalidSidesBeforeInvalidRerollOrKeep()
    {
        AssertError("d1r0kh0", "invalid dice \"d1r0kh0\" at column 1 (2 to 20 sides with a modifier)");
    }

    [Fact]
    public void RejectsPercentSidesAboveTheModifiedLimit()
    {
        AssertError("d%!", "invalid dice \"d%!\" at column 1 (2 to 20 sides with a modifier)");
    }

    [Fact]
    public void RejectsFudgeExplosionWithoutNumberedSides()
    {
        AssertError("dF!", "invalid dice \"dF!\" at column 1 (! and r need numbered sides)");
    }

    [Fact]
    public void RejectsFudgeModifierBeforeRerollAndKeepRanges()
    {
        AssertError("dFr0kh0", "invalid dice \"dFr0kh0\" at column 1 (! and r need numbered sides)");
    }

    [Fact]
    public void RejectsRerollEqualToTheNumberOfSides()
    {
        AssertError("d6r6", "invalid dice \"d6r6\" at column 1 (a reroll from 1 to 5)");
    }

    [Fact]
    public void RejectsInvalidRerollBeforeInvalidKeep()
    {
        AssertError("2d6r0kh3", "invalid dice \"2d6r0kh3\" at column 1 (a reroll from 1 to 5)");
    }

    [Fact]
    public void RejectsKeptCountOutsideDiceCount()
    {
        AssertError("4d6kh5", "invalid dice \"4d6kh5\" at column 1 (kept dice from 1 to 4)");
    }

    [Fact]
    public void RejectsFourthDiceTermAtItsFirstColumn()
    {
        AssertError("d2+d2+d2+d2", "invalid dice \"d2\" at column 10 (at most 3 dice terms)");
    }

    [Fact]
    public void DiceTermLimitIncludesDiceInsideGroups()
    {
        AssertError(
            "(d2)+(d2)+(d2)+(d2)",
            "invalid dice \"d2\" at column 17 (at most 3 dice terms)");
    }

    [Fact]
    public void LeadingZeroDiceCountIsOutsideEveryRange()
    {
        AssertError("01d6", "invalid dice \"01d6\" at column 1 (1 to 20 dice)");
    }

    [Fact]
    public void SevenDigitDiceCountIsOutsideEveryRange()
    {
        AssertError("1000000d6", "invalid dice \"1000000d6\" at column 1 (1 to 20 dice)");
    }

    [Fact]
    public void LeadingZeroSidesAreOutsideEveryRange()
    {
        AssertError("d01", "invalid dice \"d01\" at column 1 (2 to 100 sides)");
    }

    [Fact]
    public void SevenDigitSidesAreOutsideEveryRange()
    {
        AssertError("d1000000", "invalid dice \"d1000000\" at column 1 (2 to 100 sides)");
    }

    [Fact]
    public void LeadingZeroRerollIsOutsideEveryRange()
    {
        AssertError("d6r01", "invalid dice \"d6r01\" at column 1 (a reroll from 1 to 5)");
    }

    [Fact]
    public void SevenDigitRerollIsOutsideEveryRange()
    {
        AssertError("d6r1000000", "invalid dice \"d6r1000000\" at column 1 (a reroll from 1 to 5)");
    }

    [Fact]
    public void LeadingZeroKeptCountIsOutsideEveryRange()
    {
        AssertError("d6kh01", "invalid dice \"d6kh01\" at column 1 (kept dice from 1 to 1)");
    }

    [Fact]
    public void SevenDigitKeptCountIsOutsideEveryRange()
    {
        AssertError("d6kh1000000", "invalid dice \"d6kh1000000\" at column 1 (kept dice from 1 to 1)");
    }

    private static void AssertError(string text, string expectedMessage)
    {
        var exception = Assert.Throws<DieException>(() => Parser.Parse(text));

        Assert.Equal(expectedMessage, exception.Message);
    }
}
