namespace Dieodds.Tests;

public class UppercaseDiceTests
{
    [Fact]
    public void UppercaseDParsesLikeLowercaseD()
    {
        Assert.Equal(new Dice(1, 6, Mod.None, 0, KeepKind.None, 1), Parser.Parse("D6"));
        Assert.Equal(new Dice(2, 6, Mod.None, 0, KeepKind.None, 2), Parser.Parse("2D6"));
        Assert.Equal(new Dice(1, 100, Mod.None, 0, KeepKind.None, 1), Parser.Parse("D%"));
        Assert.Equal(new Dice(3, 0, Mod.None, 0, KeepKind.None, 3), Parser.Parse("3DF"));
        Assert.Equal(new Dice(3, 6, Mod.None, 0, KeepKind.High, 2), Parser.Parse("3D6kh2"));
        Assert.Equal(new Dice(2, 8, Mod.Explode, 0, KeepKind.None, 2), Parser.Parse("2D8!"));
        Assert.Equal(new Dice(1, 6, Mod.Reroll, 1, KeepKind.None, 1), Parser.Parse("D6r1"));
    }

    [Fact]
    public void UppercaseDWorksInsideExpressions()
    {
        Assert.Equal(
            new Binary('+', new Dice(2, 6, Mod.None, 0, KeepKind.None, 2), new Dice(1, 4, Mod.None, 0, KeepKind.None, 1)),
            Parser.Parse("2D6+D4"));
        Assert.Equal(
            new Binary('*', new Number(3), new Dice(1, 6, Mod.None, 0, KeepKind.None, 1)),
            Parser.Parse("3*D6"));
        Assert.Equal(
            new Group(new Dice(2, 6, Mod.None, 0, KeepKind.None, 2)),
            Parser.Parse("(2D6)"));
        Assert.Equal(
            new Binary('-', new Dice(1, 6, Mod.None, 0, KeepKind.None, 1), new Dice(1, 6, Mod.None, 0, KeepKind.None, 1)),
            Parser.Parse("d6 - D6"));
    }

    [Fact]
    public void OnlyTheDiceLetterHasAnUppercaseForm()
    {
        AssertError("D6R1", "unexpected \"R\" at column 3");
        AssertError("D6KH", "unexpected \"K\" at column 3");
        AssertError("Df", "unexpected \"f\" at column 2");
        AssertError("2D6 D6", "unexpected \"D\" at column 5");
    }

    [Fact]
    public void MalformedUppercaseDiceAreReportedAtTheirPosition()
    {
        AssertError("Dd6", "unexpected \"d\" at column 2");
        AssertError("DD6", "unexpected \"D\" at column 2");
        AssertError("D?", "unexpected \"?\" at column 2");
        AssertError("D", "unexpected end of the expression");
        AssertError("2D", "unexpected end of the expression");
    }

    [Fact]
    public void InvalidUppercaseDiceAreEchoedAsWritten()
    {
        AssertError("21D6", "invalid dice \"21D6\" at column 1 (1 to 20 dice)");
        AssertError("2D101", "invalid dice \"2D101\" at column 1 (2 to 100 sides)");
        AssertError("1+D1", "invalid dice \"D1\" at column 3 (2 to 100 sides)");
    }

    [Fact]
    public void CommandsEchoTheExpressionAsWritten()
    {
        var stats = new RecordingIo();
        Assert.Equal(0, Cli.Run(["stats", "D2"], stats));
        Assert.Equal(
            "expression: D2\noutcomes: 2\nmin: 1\nmax: 2\nmean: 1.50\nvariance: 0.25\nmedian: 1\nmode: 1, 2\n",
            stats.Output);
        Assert.Equal(string.Empty, stats.Error);

        var chance = new RecordingIo();
        Assert.Equal(0, Cli.Run(["chance", "2D6", "ge", "7"], chance));
        Assert.Equal("P(2D6 ge 7) = 58.33% (7/12)\n", chance.Output);

        var versus = new RecordingIo();
        Assert.Equal(0, Cli.Run(["versus", "D4", "d4"], versus));
        Assert.Equal("A: D4\nB: d4\nA wins: 37.50% (3/8)\ntie: 25.00% (1/4)\nB wins: 37.50% (3/8)\n", versus.Output);
    }

    [Fact]
    public void ReadmeDocumentsTheUppercaseDiceLetter()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "README.md")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        var readme = File.ReadAllText(Path.Combine(directory!.FullName, "README.md"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Contains(
            "The letter `d` of a dice term may also be written `D`: `2D6+3`, `D20`, `DF` and `D%` mean `2d6+3`, `d20`, `dF` and `d%`.",
            readme);
    }

    private static void AssertError(string text, string expectedMessage)
    {
        var exception = Assert.Throws<DieException>(() => Parser.Parse(text));

        Assert.Equal(expectedMessage, exception.Message);
    }

    private sealed class RecordingIo : IIo
    {
        public string Output { get; private set; } = string.Empty;

        public string Error { get; private set; } = string.Empty;

        public byte[] ReadStdin()
        {
            return [];
        }

        public void Out(string text)
        {
            Output += text;
        }

        public void Err(string text)
        {
            Error += text;
        }
    }
}
