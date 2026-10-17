using System.Globalization;

namespace Dieodds;

public static class Options
{
    public record Spec(string Name, int Lo, int Hi, int Fallback);

    public record Parsed(IReadOnlyDictionary<string, int> Values, IReadOnlyList<string> Args);

    public static Parsed Parse(IReadOnlyList<string> argv, IReadOnlyList<Spec> specs)
    {
        var rawValues = new Dictionary<string, string>();
        var args = new List<string>();

        for (var index = 0; index < argv.Count; index++)
        {
            var argument = argv[index];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                args.Add(argument);
                continue;
            }

            var equalsIndex = argument.IndexOf('=');
            var name = equalsIndex < 0 ? argument : argument[..equalsIndex];
            var spec = FindSpec(specs, name);
            if (spec is null)
            {
                throw new DieException($"unknown option {Text.Show(argument)}");
            }

            string value;
            if (equalsIndex >= 0)
            {
                value = argument[(equalsIndex + 1)..];
            }
            else
            {
                if (index + 1 >= argv.Count)
                {
                    throw new DieException($"{name} needs a value");
                }

                value = argv[++index];
            }

            rawValues[name] = value;
        }

        var values = new Dictionary<string, int>();
        foreach (var spec in specs)
        {
            if (!rawValues.TryGetValue(spec.Name, out var rawValue))
            {
                values[spec.Name] = spec.Fallback;
                continue;
            }

            if (!TryParseOptionValue(rawValue, out var value) || value < spec.Lo || value > spec.Hi)
            {
                var low = spec.Lo.ToString(CultureInfo.InvariantCulture);
                var high = spec.Hi.ToString(CultureInfo.InvariantCulture);
                throw new DieException(
                    $"invalid {spec.Name} {Text.Show(rawValue)} (expected an integer from {low} to {high})");
            }

            values[spec.Name] = value;
        }

        return new Parsed(values, args);
    }

    private static Spec? FindSpec(IReadOnlyList<Spec> specs, string name)
    {
        foreach (var spec in specs)
        {
            if (string.Equals(spec.Name, name, StringComparison.Ordinal))
            {
                return spec;
            }
        }

        return null;
    }

    private static bool TryParseOptionValue(string text, out int value)
    {
        value = 0;
        if (text.Length is < 1 or > 6 || text[0] is < '0' or > '9')
        {
            return false;
        }

        if (text.Length > 1 && text[0] == '0')
        {
            return false;
        }

        foreach (var character in text)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }

            value = (value * 10) + (character - '0');
        }

        return true;
    }
}
