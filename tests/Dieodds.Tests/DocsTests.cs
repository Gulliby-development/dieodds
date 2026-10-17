namespace Dieodds.Tests;

public class DocsTests
{
    [Fact]
    public void UsageExcerptMatchesHelpOutput()
    {
        AssertReadmeExcerptMatchesCli(["--help"], "## Usage");
    }

    [Fact]
    public void StatsExcerptMatchesStatsOutput()
    {
        AssertReadmeExcerptMatchesCli(["stats", "2d6+3"], "For example, `dieodds stats 2d6+3` prints:");
    }

    private static void AssertReadmeExcerptMatchesCli(string[] args, string marker)
    {
        var readme = ReadRepositoryReadme();
        var expectedOutput = ExtractTextFenceAfter(readme, marker);
        var io = new RecordingIo();

        Assert.Equal(0, Cli.Run(args, io));
        Assert.Equal(expectedOutput, io.Output);
        Assert.Equal(string.Empty, io.Error);
    }

    private static string ReadRepositoryReadme()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "README.md");
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate).Replace("\r\n", "\n", StringComparison.Ordinal);
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate README.md by walking ancestors from {AppContext.BaseDirectory}.");
    }

    private static string ExtractTextFenceAfter(string readme, string marker)
    {
        var markerIndex = readme.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0 || readme.IndexOf(marker, markerIndex + marker.Length, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException($"Expected exactly one README excerpt marker: {marker}");
        }

        const string openingFence = "\x60\x60\x60text\n";
        var openingIndex = readme.IndexOf(openingFence, markerIndex + marker.Length, StringComparison.Ordinal);
        if (openingIndex < 0 ||
            readme[(markerIndex + marker.Length)..openingIndex].Any(character => !char.IsWhiteSpace(character)))
        {
            throw new InvalidOperationException($"Could not find a text code block immediately after README marker: {marker}");
        }

        var contentStart = openingIndex + openingFence.Length;
        var closingIndex = readme.IndexOf("\n\x60\x60\x60", contentStart, StringComparison.Ordinal);
        if (closingIndex < 0)
        {
            throw new InvalidOperationException($"README text code block after marker is not closed: {marker}");
        }

        var nestedIndex = readme.IndexOf(openingFence, contentStart, StringComparison.Ordinal);
        if (nestedIndex >= 0 && nestedIndex < closingIndex)
        {
            throw new InvalidOperationException($"README text code block after marker is ambiguous: {marker}");
        }

        return readme[contentStart..(closingIndex + 1)];
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
