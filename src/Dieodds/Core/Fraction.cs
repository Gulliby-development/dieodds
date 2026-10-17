using System.Globalization;
using System.Numerics;

namespace Dieodds;

public static class Fraction
{
    public static (BigInteger Num, BigInteger Den) Reduce(BigInteger num, BigInteger den)
    {
        var gcd = BigInteger.GreatestCommonDivisor(num, den);
        if (gcd.IsZero)
        {
            return (BigInteger.Zero, BigInteger.One);
        }

        return (num / gcd, den / gcd);
    }

    public static string Decimal(BigInteger num, BigInteger den, int places)
    {
        var scale = BigInteger.Pow(10, places);
        var absoluteNumerator = BigInteger.Abs(num) * scale;
        var rounded = BigInteger.DivRem(absoluteNumerator, den, out var remainder);

        if (remainder * 2 >= den)
        {
            rounded += BigInteger.One;
        }

        var digits = rounded.ToString(CultureInfo.InvariantCulture);
        if (places == 0)
        {
            return num.Sign < 0 && !rounded.IsZero ? $"-{digits}" : digits;
        }

        if (digits.Length <= places)
        {
            digits = digits.PadLeft(places + 1, '0');
        }

        var decimalIndex = digits.Length - places;
        var whole = digits[..decimalIndex];
        var fraction = digits[decimalIndex..];
        var sign = num.Sign < 0 && !rounded.IsZero ? "-" : string.Empty;
        return $"{sign}{whole}.{fraction}";
    }

    public static string Percent(BigInteger num, BigInteger den)
    {
        return $"{Decimal(100 * num, den, 2)}%";
    }

    public static string Ratio(BigInteger num, BigInteger den)
    {
        var reduced = Reduce(num, den);
        return string.Concat(
            reduced.Num.ToString(CultureInfo.InvariantCulture),
            "/",
            reduced.Den.ToString(CultureInfo.InvariantCulture));
    }
}
