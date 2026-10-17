namespace Dieodds.Tests;

public class OptionsTests
{
    [Fact]
    public void EmptyArgumentsReturnFallbackForEverySpecAndNoArguments()
    {
        var parsed = Options.Parse(Array.Empty<string>(), WidthAndN);

        AssertParsed(parsed, 40, 0, Array.Empty<string>());
    }

    [Fact]
    public void EqualsFormParsesOptionAndPreservesPositionalArgument()
    {
        var parsed = Options.Parse(new[] { "--width=8", "d6" }, WidthAndN);

        AssertParsed(parsed, 8, 0, new[] { "d6" });
    }

    [Fact]
    public void SeparateValueConsumesDashPrefixedValueAndKeepsOtherArgumentsInOrder()
    {
        var parsed = Options.Parse(new[] { "--width", "8", "-d6", "-" }, WidthAndN);

        AssertParsed(parsed, 8, 0, new[] { "-d6", "-" });
    }

    [Fact]
    public void LastRepeatedOptionWinsWhenBothValuesAreValid()
    {
        var parsed = Options.Parse(new[] { "--width=7", "--width", "8", "d6" }, WidthAndN);

        AssertParsed(parsed, 8, 0, new[] { "d6" });
    }

    [Fact]
    public void LastRepeatedOptionWinsWhenEarlierValueIsInvalid()
    {
        var parsed = Options.Parse(new[] { "--width=bad", "--width=8" }, WidthAndN);

        AssertParsed(parsed, 8, 0, Array.Empty<string>());
    }

    [Fact]
    public void WidthLowerBoundIsAccepted()
    {
        var parsed = Options.Parse(new[] { "--width=1" }, WidthAndN);

        AssertParsed(parsed, 1, 0, Array.Empty<string>());
    }

    [Fact]
    public void WidthUpperBoundIsAccepted()
    {
        var parsed = Options.Parse(new[] { "--width=80" }, WidthAndN);

        AssertParsed(parsed, 80, 0, Array.Empty<string>());
    }

    [Fact]
    public void NLowerBoundIsAccepted()
    {
        var parsed = Options.Parse(new[] { "--n=0" }, WidthAndN);

        AssertParsed(parsed, 40, 0, Array.Empty<string>());
    }

    [Fact]
    public void NUpperBoundIsAccepted()
    {
        var parsed = Options.Parse(new[] { "--n=999999" }, WidthAndN);

        AssertParsed(parsed, 40, 999999, Array.Empty<string>());
    }

    [Fact]
    public void UnknownOptionReportsTheWholeArgument()
    {
        AssertError(new[] { "--wat=8" }, "unknown option \"--wat=8\"");
    }

    [Fact]
    public void DoubleDashAloneIsAnUnknownOption()
    {
        AssertError(new[] { "--" }, "unknown option \"--\"");
    }

    [Fact]
    public void HelpOptionIsUnknownToTheParser()
    {
        AssertError(new[] { "--help" }, "unknown option \"--help\"");
    }

    [Fact]
    public void KnownOptionWithoutValueReportsItsName()
    {
        AssertError(new[] { "--width" }, "--width needs a value");
    }

    [Fact]
    public void WidthAboveUpperBoundIsRejected()
    {
        AssertError(
            new[] { "--width=81" },
            "invalid --width \"81\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void NegativeOptionValueIsRejected()
    {
        AssertError(
            new[] { "--width=-1" },
            "invalid --width \"-1\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void ExplicitPlusSignIsRejected()
    {
        AssertError(
            new[] { "--width", "+3" },
            "invalid --width \"+3\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void LeadingZeroIsRejected()
    {
        AssertError(
            new[] { "--width=007" },
            "invalid --width \"007\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void EmptyEqualsValueIsRejected()
    {
        AssertError(
            new[] { "--width=" },
            "invalid --width \"\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void SevenDigitValueIsRejected()
    {
        AssertError(
            new[] { "--width=1000000" },
            "invalid --width \"1000000\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void AdditionalEqualsSignRemainsPartOfValue()
    {
        AssertError(
            new[] { "--width=8=9" },
            "invalid --width \"8=9\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void DashPrefixedNextArgumentIsConsumedAsValue()
    {
        AssertError(
            new[] { "--width", "--help" },
            "invalid --width \"--help\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void WhitespaceInValueIsRejectedAndSanitized()
    {
        AssertError(
            new[] { "--width= 8" },
            "invalid --width \"?8\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void NonAsciiDigitIsRejectedAndSanitized()
    {
        AssertError(
            new[] { "--width=\u0661" },
            "invalid --width \"?\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void UnknownOptionIsReportedBeforeEarlierInvalidValueIsValidated()
    {
        AssertError(
            new[] { "--width=bad", "--wat" },
            "unknown option \"--wat\"");
    }

    [Fact]
    public void MissingLaterValueIsReportedBeforeEarlierInvalidValueIsValidated()
    {
        AssertError(
            new[] { "--width=bad", "--n" },
            "--n needs a value");
    }

    [Fact]
    public void InvalidValuesAreValidatedInSpecOrder()
    {
        AssertError(
            new[] { "--n=bad", "--width=81" },
            "invalid --width \"81\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void InvalidFinalRepeatedValueIsValidatedAfterScanning()
    {
        AssertError(
            new[] { "--width=8", "--width=bad" },
            "invalid --width \"bad\" (expected an integer from 1 to 80)");
    }

    private static Options.Spec[] WidthAndN =>
    [
        new Options.Spec("--width", 1, 80, 40),
        new Options.Spec("--n", 0, 999999, 0)
    ];

    private static void AssertParsed(
        Options.Parsed parsed,
        int expectedWidth,
        int expectedN,
        IReadOnlyList<string> expectedArgs)
    {
        Assert.Equal(2, parsed.Values.Count);
        Assert.True(parsed.Values.ContainsKey("--width"));
        Assert.True(parsed.Values.ContainsKey("--n"));
        Assert.Equal(expectedWidth, parsed.Values["--width"]);
        Assert.Equal(expectedN, parsed.Values["--n"]);
        Assert.Equal(expectedArgs, parsed.Args);
    }

    private static void AssertError(string[] argv, string expectedMessage)
    {
        var exception = Assert.Throws<DieException>(() => Options.Parse(argv, WidthAndN));

        Assert.Equal(expectedMessage, exception.Message);
    }
}
