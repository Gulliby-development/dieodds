using System.Numerics;

namespace Dieodds;

public static class Die
{
    public static Dist One(int sides, Mod mod, int rerollMax)
    {
        var counts = new List<KeyValuePair<int, BigInteger>>();

        if (mod == Mod.None)
        {
            if (sides == 0)
            {
                counts.Add(new KeyValuePair<int, BigInteger>(-1, BigInteger.One));
                counts.Add(new KeyValuePair<int, BigInteger>(0, BigInteger.One));
                counts.Add(new KeyValuePair<int, BigInteger>(1, BigInteger.One));
            }
            else
            {
                for (var face = 1; face <= sides; face++)
                {
                    counts.Add(new KeyValuePair<int, BigInteger>(face, BigInteger.One));
                }
            }
        }
        else if (mod == Mod.Reroll)
        {
            for (var face = 1; face <= sides; face++)
            {
                var weight = face <= rerollMax ? rerollMax : sides + rerollMax;
                counts.Add(new KeyValuePair<int, BigInteger>(face, new BigInteger(weight)));
            }
        }
        else if (mod == Mod.Explode)
        {
            var squaredSides = new BigInteger(sides) * sides;
            for (var face = 1; face < sides; face++)
            {
                counts.Add(new KeyValuePair<int, BigInteger>(face, squaredSides));
            }

            for (var extraFace = 1; extraFace < sides; extraFace++)
            {
                counts.Add(new KeyValuePair<int, BigInteger>(
                    sides + extraFace,
                    new BigInteger(sides)));
            }

            for (var extraFace = 1; extraFace <= sides; extraFace++)
            {
                counts.Add(new KeyValuePair<int, BigInteger>(
                    (2 * sides) + extraFace,
                    BigInteger.One));
            }
        }

        return Dist.FromCounts(counts);
    }
}
