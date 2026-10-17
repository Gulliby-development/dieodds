namespace Dieodds;

public static class ChanceCommand
{
    private static readonly string[] Comparisons = ["ge", "gt", "le", "lt", "eq", "ne"];

    public static int Run(IReadOnlyList<string> argv, IIo io)
    {
        var parsed = Options.Parse(argv, []);
        if (parsed.Args.Count == 0)
        {
            throw new DieException("missing EXPR");
        }

        if (parsed.Args.Count == 1)
        {
            throw new DieException("missing OP");
        }

        if (parsed.Args.Count == 2)
        {
            throw new DieException("missing N");
        }

        if (parsed.Args.Count > 3)
        {
            throw new DieException("too many arguments");
        }

        var expressionArgument = parsed.Args[0];
        var comparison = parsed.Args[1];
        var target = parsed.Args[2];

        if (!Chance.TryParse(comparison, out var cmp))
        {
            throw new DieException(
                $"invalid comparison {Text.Show(comparison)} (expected {Text.ListOr(Comparisons)})");
        }

        if (!TryParseTarget(target, out var n))
        {
            throw new DieException(
                $"invalid N {Text.Show(target)} (expected an integer from -100000 to 100000)");
        }

        var expression = Input.Expression(io, expressionArgument);
        var node = Parser.Parse(expression);
        var distribution = Eval.Evaluate(node);
        var count = Chance.Count(distribution, cmp, n);
        var line = Chance.Line(expression, comparison, n, distribution);

        io.Out(line + "\n");
        return count.IsZero ? 1 : 0;
    }

    private static bool TryParseTarget(string text, out int value)
    {
        value = 0;
        if (text.Length == 0)
        {
            return false;
        }

        var negative = text[0] == '-';
        var firstDigit = negative ? 1 : 0;
        var digitCount = text.Length - firstDigit;
        if (digitCount is < 1 or > 6)
        {
            return false;
        }

        var first = text[firstDigit];
        if (first is < '0' or > '9' || (digitCount > 1 && first == '0') || (negative && first == '0'))
        {
            return false;
        }

        for (var index = firstDigit; index < text.Length; index++)
        {
            var digit = text[index];
            if (digit is < '0' or > '9')
            {
                return false;
            }

            value = (value * 10) + (digit - '0');
        }

        if (negative)
        {
            value = -value;
        }

        return value is >= -100000 and <= 100000;
    }
}
