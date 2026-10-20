namespace Dieodds.Tests;

public class VersionTests
{
    [Fact]
    public void VersionPrintsTheProgramNameAndTheVersionOfDirectoryBuildProps()
    {
        var io = new RecordingIo();

        Assert.Equal(0, Cli.Run(["--version"], io));

        Assert.Equal("dieodds 0.1.0\n", io.Output);
        Assert.Equal(string.Empty, io.Error);
        Assert.Equal(1, io.OutCalls);
        Assert.Equal(0, io.ErrCalls);
        Assert.Equal(0, io.StdinReads);
    }

    [Fact]
    public void VersionTogetherWithAnythingElseIsNotSpecial()
    {
        var unknownCommandIo = new RecordingIo();

        Assert.Equal(2, Cli.Run(["--version", "stats"], unknownCommandIo));

        Assert.Equal(string.Empty, unknownCommandIo.Output);
        Assert.Equal("dieodds: unknown command \"--version\"\n", unknownCommandIo.Error);
        Assert.Equal(0, unknownCommandIo.OutCalls);
        Assert.Equal(1, unknownCommandIo.ErrCalls);
        Assert.Equal(0, unknownCommandIo.StdinReads);

        var unknownOptionIo = new RecordingIo();

        Assert.Equal(2, Cli.Run(["stats", "--version"], unknownOptionIo));

        Assert.Equal(string.Empty, unknownOptionIo.Output);
        Assert.Equal("dieodds: unknown option \"--version\"\n", unknownOptionIo.Error);
        Assert.Equal(0, unknownOptionIo.OutCalls);
        Assert.Equal(1, unknownOptionIo.ErrCalls);
        Assert.Equal(0, unknownOptionIo.StdinReads);
    }

    private sealed class RecordingIo : IIo
    {
        public string Output { get; private set; } = string.Empty;

        public string Error { get; private set; } = string.Empty;

        public int OutCalls { get; private set; }

        public int ErrCalls { get; private set; }

        public int StdinReads { get; private set; }

        public byte[] ReadStdin()
        {
            StdinReads++;
            return [];
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
