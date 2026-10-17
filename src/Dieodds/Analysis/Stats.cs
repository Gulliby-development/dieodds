using System.Globalization;
using System.Numerics;

namespace Dieodds;

public static class Stats
{
    public static List<string> Lines(string expression, Dist dist)
    {
        var sum = BigInteger.Zero;
        var squareSum = BigInteger.Zero;
        var highestCount = BigInteger.Zero;

        foreach (var item in dist.Items)
        {
            var value = new BigInteger(item.Key);
            sum += value * item.Value;
            squareSum += value * value * item.Value;
            highestCount = BigInteger.Max(highestCount, item.Value);
        }

        var cumulative = BigInteger.Zero;
        var median = 0;
        foreach (var item in dist.Items)
        {
            cumulative += item.Value;
            if (cumulative * 2 >= dist.Total)
            {
                median = item.Key;
                break;
            }
        }

        var modes = dist.Items
            .Where(item => item.Value == highestCount)
            .Select(item => item.Key.ToString(CultureInfo.InvariantCulture));
        var varianceNumerator = dist.Total * squareSum - sum * sum;
        var varianceDenominator = dist.Total * dist.Total;

        return new List<string>
        {
            $"expression: {expression}",
            $"outcomes: {dist.Total.ToString(CultureInfo.InvariantCulture)}",
            $"min: {dist.Items[0].Key.ToString(CultureInfo.InvariantCulture)}",
            $"max: {dist.Items[^1].Key.ToString(CultureInfo.InvariantCulture)}",
            $"mean: {Fraction.Decimal(sum, dist.Total, 2)}",
            $"variance: {Fraction.Decimal(varianceNumerator, varianceDenominator, 2)}",
            $"median: {median.ToString(CultureInfo.InvariantCulture)}",
            $"mode: {string.Join(", ", modes)}"
        };
    }
}
