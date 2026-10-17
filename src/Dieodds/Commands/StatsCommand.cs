namespace Dieodds;

public static class StatsCommand
{
    public static int Run(IReadOnlyList<string> argv, IIo io)
    {
        var parsed = Options.Parse(argv, []);
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
        var lines = Stats.Lines(expression, distribution);

        io.Out(string.Join("\n", lines) + "\n");
        return 0;
    }
}
