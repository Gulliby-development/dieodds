namespace Dieodds;

public static class VersusCommand
{
    public static int Run(IReadOnlyList<string> argv, IIo io)
    {
        var parsed = Options.Parse(argv, []);
        if (parsed.Args.Count == 0)
        {
            throw new DieException("missing A");
        }

        if (parsed.Args.Count == 1)
        {
            throw new DieException("missing B");
        }

        if (parsed.Args.Count > 2)
        {
            throw new DieException("too many arguments");
        }

        var argumentA = parsed.Args[0];
        var argumentB = parsed.Args[1];
        if (argumentA == "-" && argumentB == "-")
        {
            throw new DieException("- can be given once");
        }

        var expressionA = Input.Expression(io, argumentA);
        var nodeA = ParseExpression(expressionA, "A");
        var expressionB = Input.Expression(io, argumentB);
        var nodeB = ParseExpression(expressionB, "B");
        var distributionA = Eval.Evaluate(nodeA);
        var distributionB = Eval.Evaluate(nodeB);
        var lines = Versus.Lines(expressionA, expressionB, distributionA, distributionB);

        io.Out(string.Join("\n", lines) + "\n");
        return 0;
    }

    private static Node ParseExpression(string expression, string operand)
    {
        try
        {
            return Parser.Parse(expression);
        }
        catch (DieException exception)
        {
            throw new DieException($"{operand}: {exception.Message}");
        }
    }
}
