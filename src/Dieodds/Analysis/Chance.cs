using System.Globalization;
using System.Numerics;

namespace Dieodds;

public enum Cmp
{
    Ge,
    Gt,
    Le,
    Lt,
    Eq,
    Ne
}

public static class Chance
{
    public static bool TryParse(string text, out Cmp cmp)
    {
        switch (text)
        {
            case "ge":
                cmp = Cmp.Ge;
                return true;
            case "gt":
                cmp = Cmp.Gt;
                return true;
            case "le":
                cmp = Cmp.Le;
                return true;
            case "lt":
                cmp = Cmp.Lt;
                return true;
            case "eq":
                cmp = Cmp.Eq;
                return true;
            case "ne":
                cmp = Cmp.Ne;
                return true;
            default:
                cmp = default;
                return false;
        }
    }

    public static BigInteger Count(Dist dist, Cmp cmp, int n)
    {
        var count = BigInteger.Zero;
        foreach (var item in dist.Items)
        {
            var matches = cmp switch
            {
                Cmp.Ge => item.Key >= n,
                Cmp.Gt => item.Key > n,
                Cmp.Le => item.Key <= n,
                Cmp.Lt => item.Key < n,
                Cmp.Eq => item.Key == n,
                Cmp.Ne => item.Key != n,
                _ => throw new ArgumentOutOfRangeException(nameof(cmp), cmp, null)
            };

            if (matches)
            {
                count += item.Value;
            }
        }

        return count;
    }

    public static string Line(string expression, string op, int n, Dist dist)
    {
        var count = TryParse(op, out var cmp)
            ? Count(dist, cmp, n)
            : throw new ArgumentException("Unsupported comparison operator.", nameof(op));
        var number = n.ToString(CultureInfo.InvariantCulture);
        var percent = Fraction.Percent(count, dist.Total);
        var ratio = Fraction.Ratio(count, dist.Total);

        return $"P({expression} {op} {number}) = {percent} ({ratio})";
    }
}
