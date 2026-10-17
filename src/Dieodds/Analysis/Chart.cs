using System.Globalization;
using System.Numerics;

namespace Dieodds;

public static class Chart
{
    public static List<string> Lines(Dist dist, int width)
    {
        var maximumCount = dist.Items.Max(item => item.Value);
        var rows = new List<IReadOnlyList<string>>(dist.Items.Count);

        foreach (var item in dist.Items)
        {
            var scaledLength = item.Value * width / maximumCount;
            var barLength = scaledLength < BigInteger.One ? 1 : (int)scaledLength;

            rows.Add(
            [
                item.Key.ToString(CultureInfo.InvariantCulture),
                Fraction.Percent(item.Value, dist.Total),
                new string('#', barLength)
            ]);
        }

        return Text.RenderTable(["value", "chance", "bar"], rows);
    }
}
