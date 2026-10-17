namespace Dieodds.Tests;

public class CommandTests
{
    [Fact]
    public void StatsMatchesTheCompleteTwoD6WorkedOutput()
    {
        var io = new RecordingIo();

        var result = StatsCommand.Run(["2d6+3"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "expression: 2d6+3\noutcomes: 36\nmin: 5\nmax: 15\nmean: 10.00\nvariance: 5.83\nmedian: 10\nmode: 10\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
        Assert.Equal(0, io.ErrCalls);
    }

    [Fact]
    public void StatsMatchesTheCompleteTiedModeWorkedOutput()
    {
        var io = new RecordingIo();

        var result = StatsCommand.Run(["d4"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "expression: d4\noutcomes: 4\nmin: 1\nmax: 4\nmean: 2.50\nvariance: 1.25\nmedian: 2\nmode: 1, 2, 3, 4\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void StatsMatchesTheCompleteKeepHighestWorkedOutput()
    {
        var io = new RecordingIo();

        var result = StatsCommand.Run(["3d6kh2"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "expression: 3d6kh2\noutcomes: 216\nmin: 2\nmax: 12\nmean: 8.46\nvariance: 4.91\nmedian: 9\nmode: 9\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void StatsMatchesTheCompleteSignedExpressionWorkedOutput()
    {
        var io = new RecordingIo();

        var result = StatsCommand.Run(["-d6*2+1"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "expression: -d6*2+1\noutcomes: 6\nmin: -11\nmax: -1\nmean: -6.00\nvariance: 11.67\nmedian: -7\nmode: -11, -9, -7, -5, -3, -1\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void StatsMatchesTheCompleteGroupedExpressionWorkedOutput()
    {
        var io = new RecordingIo();

        var result = StatsCommand.Run(["(d4 + 1) * 3"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "expression: (d4 + 1) * 3\noutcomes: 4\nmin: 6\nmax: 15\nmean: 10.50\nvariance: 11.25\nmedian: 9\nmode: 6, 9, 12, 15\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void ChartMatchesTheCompleteTwoD4WorkedOutput()
    {
        var io = new RecordingIo();

        var result = ChartCommand.Run(["--width", "8", "2d4"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "value  chance  bar\n2      6.25%   ##\n3      12.50%  ####\n4      18.75%  ######\n5      25.00%  ########\n6      18.75%  ######\n7      12.50%  ####\n8      6.25%   ##\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void ChartMatchesTheCompleteExplodingDieWorkedOutput()
    {
        var io = new RecordingIo();

        var result = ChartCommand.Run(["--width=8", "d2!"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "value  chance  bar\n1      50.00%  ########\n3      25.00%  ####\n5      12.50%  ##\n6      12.50%  ##\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void ChartMatchesTheCompleteRerollWorkedOutput()
    {
        var io = new RecordingIo();

        var result = ChartCommand.Run(["--width", "8", "d3r1"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "value  chance  bar\n1      11.11%  ##\n2      44.44%  ########\n3      44.44%  ########\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void ChartUsesWidthFortyByDefault()
    {
        var io = new RecordingIo();

        var result = ChartCommand.Run(["d2"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "value  chance  bar\n1      50.00%  ########################################\n2      50.00%  ########################################\n",
            io.Output);
    }

    [Fact]
    public void ChartAcceptsTheLowerWidthBoundary()
    {
        var io = new RecordingIo();

        var result = ChartCommand.Run(["--width=1", "d2"], io);

        Assert.Equal(0, result);
        Assert.Equal("value  chance  bar\n1      50.00%  #\n2      50.00%  #\n", io.Output);
    }

    [Fact]
    public void ChartAcceptsTheUpperWidthBoundary()
    {
        var io = new RecordingIo();

        var result = ChartCommand.Run(["--width=80", "d2"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "value  chance  bar\n1      50.00%  ################################################################################\n2      50.00%  ################################################################################\n",
            io.Output);
    }

    [Fact]
    public void ChartSeparateAndEqualsWidthFormsAgree()
    {
        var equalsIo = new RecordingIo();
        var separateIo = new RecordingIo();

        Assert.Equal(0, ChartCommand.Run(["--width=8", "2d4"], equalsIo));
        Assert.Equal(0, ChartCommand.Run(["--width", "8", "2d4"], separateIo));

        Assert.Equal(equalsIo.Output, separateIo.Output);
        Assert.Equal(1, equalsIo.OutCalls);
        Assert.Equal(1, separateIo.OutCalls);
    }

    [Fact]
    public void ChartUsesTheLastRepeatedWidthEvenWhenTheEarlierValueIsInvalid()
    {
        var io = new RecordingIo();

        var result = ChartCommand.Run(["--width=bad", "--width=1", "d2"], io);

        Assert.Equal(0, result);
        Assert.Equal("value  chance  bar\n1      50.00%  #\n2      50.00%  #\n", io.Output);
        Assert.Equal(string.Empty, io.Error);
    }

    [Fact]
    public void ChanceMatchesTheAdvantageWorkedOutput()
    {
        var io = new RecordingIo();

        var result = ChanceCommand.Run(["2d20kh1", "ge", "15"], io);

        Assert.Equal(0, result);
        Assert.Equal("P(2d20kh1 ge 15) = 51.00% (51/100)\n", io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void ChanceReturnsOneAndPrintsTheZeroProbabilityWorkedOutput()
    {
        var io = new RecordingIo();

        var result = ChanceCommand.Run(["d6", "gt", "6"], io);

        Assert.Equal(1, result);
        Assert.Equal("P(d6 gt 6) = 0.00% (0/1)\n", io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void ChanceMatchesTheFudgeDiceWorkedOutput()
    {
        var io = new RecordingIo();

        var result = ChanceCommand.Run(["4dF", "eq", "0"], io);

        Assert.Equal(0, result);
        Assert.Equal("P(4dF eq 0) = 23.46% (19/81)\n", io.Output);
    }

    [Fact]
    public void ChanceDoesNotReturnOneForANonzeroShareRoundedToZero()
    {
        var io = new RecordingIo();

        var result = ChanceCommand.Run(["20d2", "eq", "20"], io);

        Assert.Equal(0, result);
        Assert.Equal("P(20d2 eq 20) = 0.00% (1/1048576)\n", io.Output);
    }

    [Theory]
    [InlineData("ge", "P(2d2 ge 3) = 75.00% (3/4)\n", 0)]
    [InlineData("gt", "P(2d2 gt 3) = 25.00% (1/4)\n", 0)]
    [InlineData("le", "P(2d2 le 3) = 75.00% (3/4)\n", 0)]
    [InlineData("lt", "P(2d2 lt 3) = 25.00% (1/4)\n", 0)]
    [InlineData("eq", "P(2d2 eq 3) = 50.00% (1/2)\n", 0)]
    [InlineData("ne", "P(2d2 ne 3) = 50.00% (1/2)\n", 0)]
    public void ChanceSupportsAllSixComparisons(string op, string expected, int expectedExit)
    {
        var io = new RecordingIo();

        Assert.Equal(expectedExit, ChanceCommand.Run(["2d2", op, "3"], io));
        Assert.Equal(expected, io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void ChanceAcceptsTheNegativeTargetBoundary()
    {
        var io = new RecordingIo();

        Assert.Equal(0, ChanceCommand.Run(["d2", "ge", "-100000"], io));
        Assert.Equal("P(d2 ge -100000) = 100.00% (1/1)\n", io.Output);
    }

    [Fact]
    public void ChanceAcceptsThePositiveTargetBoundary()
    {
        var io = new RecordingIo();

        Assert.Equal(1, ChanceCommand.Run(["d2", "eq", "100000"], io));
        Assert.Equal("P(d2 eq 100000) = 0.00% (0/1)\n", io.Output);
    }

    [Theory]
    [InlineData("-0", "-0")]
    [InlineData("+3", "+3")]
    [InlineData("007", "007")]
    [InlineData(" 3", "?3")]
    [InlineData("", "")]
    [InlineData("1.5", "1.5")]
    [InlineData("\u0661", "?")]
    [InlineData("100001", "100001")]
    [InlineData("-100001", "-100001")]
    public void ChanceRejectsEveryNoncanonicalOrOutOfRangeTarget(string target, string shown)
    {
        AssertFailure(
            io => ChanceCommand.Run(["d6", "eq", target], io),
            $"invalid N \"{shown}\" (expected an integer from -100000 to 100000)");
    }

    [Theory]
    [InlineData("GE")]
    [InlineData(">=")]
    [InlineData("")]
    public void ChanceRejectsUnsupportedComparisonTokens(string comparison)
    {
        AssertFailure(
            io => ChanceCommand.Run(["d6", comparison, "3"], io),
            $"invalid comparison \"{comparison}\" (expected ge, gt, le, lt, eq or ne)");
    }

    [Fact]
    public void VersusMatchesThePairwiseWorkedOutput()
    {
        var io = new RecordingIo();

        var result = VersusCommand.Run(["d20+5", "2d10+8"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "A: d20+5\nB: 2d10+8\nA wins: 30.50% (61/200)\ntie: 4.70% (47/1000)\nB wins: 64.80% (81/125)\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void VersusPrintsAllTieAndZeroLines()
    {
        var io = new RecordingIo();

        var result = VersusCommand.Run(["0", "0"], io);

        Assert.Equal(0, result);
        Assert.Equal(
            "A: 0\nB: 0\nA wins: 0.00% (0/1)\ntie: 100.00% (1/1)\nB wins: 0.00% (0/1)\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
    }

    [Fact]
    public void StatsRejectsMissingExpressionBeforeReadingStdin()
    {
        AssertFailure(io => StatsCommand.Run([], io), "missing EXPR");
    }

    [Fact]
    public void VersusRejectsMissingA()
    {
        AssertFailure(io => VersusCommand.Run([], io), "missing A");
    }

    [Fact]
    public void VersusRejectsMissingB()
    {
        AssertFailure(io => VersusCommand.Run(["d6"], io), "missing B");
    }

    [Fact]
    public void ChanceChecksMissingPositionsInOrder()
    {
        AssertFailure(io => ChanceCommand.Run([], io), "missing EXPR");
        AssertFailure(io => ChanceCommand.Run(["d6"], io), "missing OP");
        AssertFailure(io => ChanceCommand.Run(["d6", "ge"], io), "missing N");
    }

    [Fact]
    public void CommandsRejectExcessArgumentsBeforeParsingExpressions()
    {
        AssertFailure(io => StatsCommand.Run(["1+-2", "extra"], io), "too many arguments");
        AssertFailure(io => ChartCommand.Run(["d6", "extra"], io), "too many arguments");
        AssertFailure(io => ChanceCommand.Run(["d6", "ge", "3", "extra"], io), "too many arguments");
        AssertFailure(io => VersusCommand.Run(["d6", "d6", "extra"], io), "too many arguments");
    }

    [Fact]
    public void OptionsAreValidatedBeforeMissingArguments()
    {
        AssertFailure(
            io => ChartCommand.Run(["--width=81"], io),
            "invalid --width \"81\" (expected an integer from 1 to 80)");
        AssertFailure(io => ChartCommand.Run(["--width"], io), "--width needs a value");
        AssertFailure(io => StatsCommand.Run(["--help"], io), "unknown option \"--help\"");
        AssertFailure(io => ChanceCommand.Run(["--help"], io), "unknown option \"--help\"");
        AssertFailure(io => VersusCommand.Run(["--width=8", "d6", "d6"], io), "unknown option \"--width=8\"");
    }

    [Fact]
    public void ChanceChecksComparisonBeforeTargetAndTargetBeforeInput()
    {
        AssertFailure(
            io => ChanceCommand.Run(["-", "bad", "007"], io),
            "invalid comparison \"bad\" (expected ge, gt, le, lt, eq or ne)");
        AssertFailure(
            io => ChanceCommand.Run(["-", "ge", "007"], io),
            "invalid N \"007\" (expected an integer from -100000 to 100000)");
    }

    [Fact]
    public void VersusRejectsTwoStdinOperandsWithoutReadingStdin()
    {
        var io = new RecordingIo();

        var exception = Assert.Throws<DieException>(() => VersusCommand.Run(["-", "-"], io));

        Assert.Equal("- can be given once", exception.Message);
        Assert.Equal(0, io.StdinReads);
        AssertNoWrites(io);
    }

    [Fact]
    public void VersusParsesAAndPrefixesItsParserErrorBeforeReadingB()
    {
        var io = new RecordingIo();

        var exception = Assert.Throws<DieException>(() => VersusCommand.Run(["1+-2", "-"], io));

        Assert.Equal("A: unexpected \"-\" at column 3", exception.Message);
        Assert.Equal(0, io.StdinReads);
        AssertNoWrites(io);
    }

    [Fact]
    public void VersusPrefixesOnlyTheParserErrorForB()
    {
        AssertFailure(
            io => VersusCommand.Run(["d6", "1+-2"], io),
            "B: unexpected \"-\" at column 3");
    }

    [Fact]
    public void VersusDoesNotPrefixInputErrorsWithAnOperandName()
    {
        var io = new RecordingIo([0xff]);

        var exception = Assert.Throws<DieException>(() => VersusCommand.Run(["-", "d6"], io));

        Assert.Equal("<stdin>: input is not valid UTF-8", exception.Message);
        Assert.Equal(1, io.StdinReads);
        AssertNoWrites(io);
    }

    [Fact]
    public void CommandsDoNotWritePartialOutputWhenInputOrParsingFails()
    {
        var invalidInput = new RecordingIo([0xff]);

        var inputException = Assert.Throws<DieException>(() => StatsCommand.Run(["-"], invalidInput));
        Assert.Equal("<stdin>: input is not valid UTF-8", inputException.Message);
        AssertNoWrites(invalidInput);

        var invalidExpression = new RecordingIo();

        var parseException = Assert.Throws<DieException>(() => ChartCommand.Run(["1+-2"], invalidExpression));
        Assert.Equal("unexpected \"-\" at column 3", parseException.Message);
        AssertNoWrites(invalidExpression);
    }

    private static void AssertFailure(Func<RecordingIo, int> run, string expectedMessage)
    {
        var io = new RecordingIo();

        var exception = Assert.Throws<DieException>(() => run(io));

        Assert.Equal(expectedMessage, exception.Message);
        AssertNoWrites(io);
    }

    private static void AssertNoWrites(RecordingIo io)
    {
        Assert.Equal(string.Empty, io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(0, io.OutCalls);
        Assert.Equal(0, io.ErrCalls);
    }

    private sealed class RecordingIo : IIo
    {
        private readonly byte[] _stdinBytes;

        public RecordingIo(byte[]? stdinBytes = null)
        {
            _stdinBytes = stdinBytes ?? [];
        }

        public string Output { get; private set; } = string.Empty;

        public string Error { get; private set; } = string.Empty;

        public int OutCalls { get; private set; }

        public int ErrCalls { get; private set; }

        public int StdinReads { get; private set; }

        public byte[] ReadStdin()
        {
            StdinReads++;
            return _stdinBytes;
        }

        public void Out(string text)
        {
            OutCalls++;
            Output += text;
        }

        public void Err(string text)
        {
            ErrCalls++;
            Error += text;
        }
    }
}
