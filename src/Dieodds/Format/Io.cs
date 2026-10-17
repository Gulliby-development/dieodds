using System.Text;

namespace Dieodds;

public interface IIo
{
    byte[] ReadStdin();

    void Out(string text);

    void Err(string text);
}

public static class Input
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static string Expression(IIo io, string arg)
    {
        ArgumentNullException.ThrowIfNull(io);
        ArgumentNullException.ThrowIfNull(arg);

        if (arg != "-")
        {
            return TrimSpacesAndTabs(arg);
        }

        string text;
        try
        {
            text = StrictUtf8.GetString(io.ReadStdin());
        }
        catch (DecoderFallbackException)
        {
            throw new DieException("<stdin>: input is not valid UTF-8");
        }

        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        var expressions = new List<string>();
        var lineStart = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] != '\n')
            {
                continue;
            }

            var line = text[lineStart..index];
            if (line.EndsWith('\r'))
            {
                line = line[..^1];
            }

            AddNonblank(expressions, line);
            lineStart = index + 1;
        }

        AddNonblank(expressions, text[lineStart..]);

        return expressions.Count switch
        {
            0 => throw new DieException("<stdin>: no expression"),
            > 1 => throw new DieException("<stdin>: expected one expression"),
            _ => TrimSpacesAndTabs(expressions[0])
        };
    }

    private static void AddNonblank(List<string> expressions, string line)
    {
        if (TrimSpacesAndTabs(line).Length > 0)
        {
            expressions.Add(line);
        }
    }

    private static string TrimSpacesAndTabs(string text)
    {
        return text.Trim(' ', '\t');
    }
}
