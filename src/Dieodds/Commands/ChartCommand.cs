namespace Dieodds;

public static class ChartCommand
{
    private static readonly IReadOnlyList<Options.Spec> Specs =
    [
        new("--width", 1, 80, 40)
    ];

    public static int Run(IReadOnlyList<string> argv, IIo io)
    {
        var parsed = Options.Parse(argv, Specs);
        if (parsed.Args.Count == 0)
        {
            throw new DieException("missing EXPR");
        }

        if (parsed.Args.Count > 1)
        {
            throw new DieException("too many arguments");
        }

        var expression = Input.Expression(io, parsed.Args[0]);
        var node = Parser.Parse(expression);
        var distribution = Eval.Evaluate(node);
        var lines = Chart.Lines(distribution, parsed.Values["--width"]);

        io.Out(string.Join("\n", lines) + "\n");
        return 0;
    }
}
