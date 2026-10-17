using System.Globalization;
using System.Numerics;

namespace Dieodds;

public static class Versus
{
    public static (BigInteger Win, BigInteger Tie, BigInteger Lose) Counts(Dist a, Dist b)
    {
        var win = BigInteger.Zero;
        var tie = BigInteger.Zero;
        var lose = BigInteger.Zero;

        foreach (var left in a.Items)
        {
            foreach (var right in b.Items)
            {
                var pairCount = left.Value * right.Value;
                if (left.Key > right.Key)
                {
                    win += pairCount;
                }
                else if (left.Key == right.Key)
                {
                    tie += pairCount;
                }
                else
                {
                    lose += pairCount;
                }
            }
        }

        return (win, tie, lose);
    }

    public static List<string> Lines(string a, string b, Dist da, Dist db)
    {
        var (win, tie, lose) = Counts(da, db);
        var total = da.Total * db.Total;

        return
        [
            $"A: {a}",
            $"B: {b}",
            $"A wins: {Fraction.Percent(win, total)} ({Fraction.Ratio(win, total)})",
            $"tie: {Fraction.Percent(tie, total)} ({Fraction.Ratio(tie, total)})",
            $"B wins: {Fraction.Percent(lose, total)} ({Fraction.Ratio(lose, total)})"
        ];
    }
}
