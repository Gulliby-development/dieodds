using System.Diagnostics;
using System.Text;

namespace Dieodds.Tests;

public class CliTests
{


    [Fact]
    public void HelpPrintsTheExactUsageAndUsageOmitsItsFinalLineFeed()
    {
        var io = AssertSuccess(
            ["--help"],
            "usage: dieodds stats EXPR\n       dieodds chart [--width N] EXPR\n       dieodds chance EXPR OP N\n       dieodds versus A B\n");

        Assert.Equal(
            "usage: dieodds stats EXPR\n       dieodds chart [--width N] EXPR\n       dieodds chance EXPR OP N\n       dieodds versus A B",
            Cli.Usage);
        Assert.Equal(0, io.StdinReads);
    }

    [Fact]
    public void MissingCommandUsesOneExactErrorLine()
    {
        AssertFailure([], "missing command (stats, chart, chance or versus)");
    }

    [Fact]
    public void UnknownCommandReportsBogusExactly()
    {
        AssertFailure(["bogus"], "unknown command \"bogus\"");
    }

    [Fact]
    public void UnknownCommandSanitizesItsText()
    {
        AssertFailure(["bad\ncommand"], "unknown command \"bad?command\"");
    }

    [Fact]
    public void HelpBeforeACommandIsAnUnknownCommand()
    {
        AssertFailure(["--help", "stats"], "unknown command \"--help\"");
    }

    [Fact]
    public void HelpAfterACommandIsAnUnknownOption()
    {
        AssertFailure(["stats", "--help"], "unknown option \"--help\"");
    }

    [Fact]
    public void CliDispatchesStatsAndPrintsAllLines()
    {
        AssertSuccess(
            ["stats", "2d6+3"],
            "expression: 2d6+3\noutcomes: 36\nmin: 5\nmax: 15\nmean: 10.00\nvariance: 5.83\nmedian: 10\nmode: 10\n");
    }

    [Fact]
    public void CliDispatchesChartAndUsesItsDefaultWidth()
    {
        AssertSuccess(
            ["chart", "d2"],
            "value  chance  bar\n1      50.00%  ########################################\n2      50.00%  ########################################\n");
    }

    [Fact]
    public void CliDispatchesChanceAndPreservesItsNoMatchExitCode()
    {
        AssertSuccess(["chance", "d6", "gt", "6"], "P(d6 gt 6) = 0.00% (0/1)\n", 1);
    }

    [Fact]
    public void CliDispatchesVersusAndPrintsAllPairwiseLines()
    {
        AssertSuccess(
            ["versus", "0", "0"],
            "A: 0\nB: 0\nA wins: 0.00% (0/1)\ntie: 100.00% (1/1)\nB wins: 0.00% (0/1)\n");
    }

    [Fact]
    public void CliReportsUnexpectedEndForTheWorkedStatsFailure()
    {
        AssertFailure(["stats", "2d6+"], "unexpected end of the expression");
    }

    [Fact]
    public void CliReportsInvalidMultiplicationForTheWorkedStatsFailure()
    {
        AssertFailure(["stats", "d6*d6"], "* needs a number on one side at column 3");
    }

    [Fact]
    public void CliReportsInvalidDiceCountForTheWorkedStatsFailure()
    {
        AssertFailure(["stats", "21d6"], "invalid dice \"21d6\" at column 1 (1 to 20 dice)");
    }

    [Fact]
    public void CliReportsInvalidKeptDiceForTheWorkedStatsFailure()
    {
        AssertFailure(["stats", "4d6kh5"], "invalid dice \"4d6kh5\" at column 1 (kept dice from 1 to 4)");
    }

    [Fact]
    public void CliReportsInvalidTargetForTheWorkedChanceFailure()
    {
        AssertFailure(
            ["chance", "2d6", "ge", "007"],
            "invalid N \"007\" (expected an integer from -100000 to 100000)");
    }

    [Fact]
    public void CliReportsInvalidWidthForTheWorkedChartFailure()
    {
        AssertFailure(
            ["chart", "--width", "81", "d6"],
            "invalid --width \"81\" (expected an integer from 1 to 80)");
    }

    [Fact]
    public void CliReportsTheBPrefixForTheWorkedVersusFailure()
    {
        AssertFailure(["versus", "d6", "1+-2"], "B: unexpected \"-\" at column 3");
    }

    [Fact]
    public void ChartOptionErrorPrecedesMissingExpressionAndInput()
    {
        var io = new RecordingIo();

        Assert.Equal(2, Cli.Run(["chart", "--width=81"], io));

        Assert.Equal(string.Empty, io.Output);
        Assert.Equal("dieodds: invalid --width \"81\" (expected an integer from 1 to 80)\n", io.Error);
        Assert.Equal(0, io.OutCalls);
        Assert.Equal(1, io.ErrCalls);
        Assert.Equal(0, io.StdinReads);
    }

    [Fact]
    public void ChanceComparisonErrorPrecedesTargetValidationAndInput()
    {
        var io = new RecordingIo();

        Assert.Equal(2, Cli.Run(["chance", "-", "bad", "007"], io));

        Assert.Equal(string.Empty, io.Output);
        Assert.Equal(
            "dieodds: invalid comparison \"bad\" (expected ge, gt, le, lt, eq or ne)\n",
            io.Error);
        Assert.Equal(0, io.OutCalls);
        Assert.Equal(1, io.ErrCalls);
        Assert.Equal(0, io.StdinReads);
    }

    [Fact]
    public void ChanceTargetErrorPrecedesInput()
    {
        var io = new RecordingIo();

        Assert.Equal(2, Cli.Run(["chance", "-", "ge", "007"], io));

        Assert.Equal(
            "dieodds: invalid N \"007\" (expected an integer from -100000 to 100000)\n",
            io.Error);
        Assert.Equal(0, io.StdinReads);
        Assert.Equal(0, io.OutCalls);
    }

    [Fact]
    public void ExcessArgumentsPrecedeParsing()
    {
        AssertFailure(["stats", "1+-2", "extra"], "too many arguments");
    }

    [Fact]
    public void InputTrimsOnlySpacesAndTabsFromAnOrdinaryArgument()
    {
        var io = new RecordingIo();

        var expression = Input.Expression(io, " \td4 + 1 \t");

        Assert.Equal("d4 + 1", expression);
        Assert.Equal(0, io.StdinReads);
    }

    [Fact]
    public void InputSkipsOneLeadingBom()
    {
        var io = new RecordingIo(Encoding.UTF8.GetBytes("\uFEFFd6\n"));

        Assert.Equal("d6", Input.Expression(io, "-"));
        Assert.Equal(1, io.StdinReads);
    }

    [Fact]
    public void InputAcceptsCrLfAndIgnoresBlankLinesWhilePreservingInternalSpaces()
    {
        var io = new RecordingIo(Encoding.UTF8.GetBytes("\r\n \t\r\n  d4 + 1 \t\r\n \r\n"));

        Assert.Equal("d4 + 1", Input.Expression(io, "-"));
        Assert.Equal(1, io.StdinReads);
    }

    [Fact]
    public void InputAcceptsLfAndTrimsTheSingleExpression()
    {
        var io = new RecordingIo(Encoding.UTF8.GetBytes("\n\td6\t\n"));

        Assert.Equal("d6", Input.Expression(io, "-"));
        Assert.Equal(1, io.StdinReads);
    }

    [Fact]
    public void InputRejectsEmptyStdin()
    {
        AssertInputFailure([], "<stdin>: no expression");
    }

    [Fact]
    public void InputRejectsStdinContainingOnlySpaceAndTabLines()
    {
        AssertInputFailure(Encoding.UTF8.GetBytes(" \t\n\t "), "<stdin>: no expression");
    }

    [Fact]
    public void InputRejectsTwoNonblankExpressions()
    {
        AssertInputFailure(Encoding.UTF8.GetBytes("d6\n d4 \n"), "<stdin>: expected one expression");
    }

    [Fact]
    public void InputRejectsMalformedUtf8Bytes()
    {
        AssertInputFailure([0x32, 0x64, 0x36, 0xff, 0x0a], "<stdin>: input is not valid UTF-8");
    }

    [Fact]
    public void CliReadsAValidStdinExpressionAndUsesItsTrimmedLabel()
    {
        var io = new RecordingIo(Encoding.UTF8.GetBytes("\uFEFF d2 \r\n"));

        Assert.Equal(
            0,
            Cli.Run(["stats", "-"], io));

        Assert.Equal(
            "expression: d2\noutcomes: 2\nmin: 1\nmax: 2\nmean: 1.50\nvariance: 0.25\nmedian: 1\nmode: 1, 2\n",
            io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.StdinReads);
        Assert.Equal(1, io.OutCalls);
        Assert.Equal(0, io.ErrCalls);
    }

    [Fact]
    public void CliReportsMalformedStdinWithoutPartialOutput()
    {
        var io = new RecordingIo([0x32, 0x64, 0x36, 0xff, 0x0a]);

        Assert.Equal(2, Cli.Run(["stats", "-"], io));

        Assert.Equal(string.Empty, io.Output);
        Assert.Equal("dieodds: <stdin>: input is not valid UTF-8\n", io.Error);
        Assert.Equal(1, io.StdinReads);
        Assert.Equal(0, io.OutCalls);
        Assert.Equal(1, io.ErrCalls);
    }

    [Fact]
    public void VersusParsesAAndReportsItsParserErrorBeforeReadingB()
    {
        var io = new RecordingIo(Encoding.UTF8.GetBytes("d6\n"));

        Assert.Equal(2, Cli.Run(["versus", "1+-2", "-"], io));

        Assert.Equal(string.Empty, io.Output);
        Assert.Equal("dieodds: A: unexpected \"-\" at column 3\n", io.Error);
        Assert.Equal(0, io.StdinReads);
        Assert.Equal(0, io.OutCalls);
        Assert.Equal(1, io.ErrCalls);
    }

    [Fact]
    public void VersusPrefixesTheBParserErrorButDoesNotPrefixBInputErrors()
    {
        AssertFailure(["versus", "d6", "1+-2"], "B: unexpected \"-\" at column 3");

        var io = new RecordingIo([0xff]);

        Assert.Equal(2, Cli.Run(["versus", "-", "d6"], io));
        Assert.Equal("dieodds: <stdin>: input is not valid UTF-8\n", io.Error);
        Assert.Equal(1, io.StdinReads);
        Assert.Equal(0, io.OutCalls);
    }

    [Fact]
    public void VersusRejectsTwoStdinArgumentsBeforeReadingEither()
    {
        var io = new RecordingIo();

        Assert.Equal(2, Cli.Run(["versus", "-", "-"], io));

        Assert.Equal("dieodds: - can be given once\n", io.Error);
        Assert.Equal(0, io.StdinReads);
        Assert.Equal(0, io.OutCalls);
    }

    [Fact]
    public async Task ProcessRejectsMalformedUtf8OnRealStandardInput()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var assemblyPath = Path.Combine(baseDirectory, "dieodds.dll");
        Assert.True(File.Exists(assemblyPath), $"Expected executable assembly at {assemblyPath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = baseDirectory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("dieodds.dll");
        startInfo.ArgumentList.Add("stats");
        startInfo.ArgumentList.Add("-");

        using var process = new Process { StartInfo = startInfo };
        Assert.True(process.Start());

        var standardOutputTask = ReadBytesAsync(process.StandardOutput.BaseStream);
        var standardErrorTask = ReadBytesAsync(process.StandardError.BaseStream);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            await process.StandardInput.BaseStream.WriteAsync(
                new byte[] { 0x32, 0x64, 0x36, 0xff, 0x0a }.AsMemory(),
                timeout.Token);
            await process.StandardInput.BaseStream.FlushAsync(timeout.Token);
            process.StandardInput.Close();

            await process.WaitForExitAsync(timeout.Token);
            var output = await Task.WhenAll(standardOutputTask, standardErrorTask);

            Assert.Equal(2, process.ExitCode);
            Assert.Equal(Array.Empty<byte>(), output[0]);
            Assert.Equal(
                Encoding.UTF8.GetBytes("dieodds: <stdin>: input is not valid UTF-8\n"),
                output[1]);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }
    }

    private static RecordingIo AssertSuccess(string[] args, string expectedOutput, int expectedExitCode = 0)
    {
        var io = new RecordingIo();

        Assert.Equal(expectedExitCode, Cli.Run(args, io));
        Assert.Equal(expectedOutput, io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
        Assert.Equal(0, io.ErrCalls);

        return io;
    }

    private static void AssertFailure(string[] args, string expectedMessage)
    {
        var io = new RecordingIo();

        Assert.Equal(2, Cli.Run(args, io));

        Assert.Equal(string.Empty, io.Output);
        Assert.Equal($"dieodds: {expectedMessage}\n", io.Error);
        Assert.Equal(0, io.OutCalls);
        Assert.Equal(1, io.ErrCalls);
    }

    private static void AssertInputFailure(byte[] stdin, string expectedMessage)
    {
        var io = new RecordingIo(stdin);

        var exception = Assert.Throws<DieException>(() => Input.Expression(io, "-"));

        Assert.Equal(expectedMessage, exception.Message);
        Assert.Equal(1, io.StdinReads);
    }

    private static async Task<byte[]> ReadBytesAsync(Stream stream)
    {
        using var output = new MemoryStream();
        await stream.CopyToAsync(output);
        return output.ToArray();
    }

    private sealed class RecordingIo : IIo
    {
        private readonly byte[] _stdin;

        public RecordingIo(byte[]? stdin = null)
        {
            _stdin = stdin ?? [];
        }

        public string Output { get; private set; } = string.Empty;

        public string Error { get; private set; } = string.Empty;

        public int OutCalls { get; private set; }

        public int ErrCalls { get; private set; }

        public int StdinReads { get; private set; }

        public byte[] ReadStdin()
        {
            StdinReads++;
            return _stdin;
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
