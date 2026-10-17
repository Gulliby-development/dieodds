using System.Numerics;

namespace Dieodds;

public sealed class Dist
{
    private readonly IReadOnlyList<KeyValuePair<int, BigInteger>> _items;

    private Dist(IEnumerable<KeyValuePair<int, BigInteger>> items)
    {
        var ownedItems = items.ToArray();
        _items = Array.AsReadOnly(ownedItems);

        var total = BigInteger.Zero;
        foreach (var item in ownedItems)
        {
            total += item.Value;
        }

        Total = total;
    }

    public IReadOnlyList<KeyValuePair<int, BigInteger>> Items => _items;

    public BigInteger Total { get; }

    public static Dist FromCounts(IEnumerable<KeyValuePair<int, BigInteger>> counts)
    {
        var merged = new Dictionary<int, BigInteger>();
        foreach (var item in counts)
        {
            if (merged.TryGetValue(item.Key, out var current))
            {
                merged[item.Key] = current + item.Value;
            }
            else
            {
                merged.Add(item.Key, item.Value);
            }
        }

        var items = merged
            .Where(item => item.Value > BigInteger.Zero)
            .OrderBy(item => item.Key)
            .Select(item => new KeyValuePair<int, BigInteger>(item.Key, item.Value));

        return new Dist(items);
    }

    public static Dist Constant(int value)
    {
        return new Dist(new[] { new KeyValuePair<int, BigInteger>(value, BigInteger.One) });
    }

    public Dist Add(Dist other)
    {
        var counts = new List<KeyValuePair<int, BigInteger>>();
        foreach (var left in Items)
        {
            foreach (var right in other.Items)
            {
                counts.Add(new KeyValuePair<int, BigInteger>(left.Key + right.Key, left.Value * right.Value));
            }
        }

        return FromCounts(counts);
    }

    public Dist Subtract(Dist other)
    {
        var counts = new List<KeyValuePair<int, BigInteger>>();
        foreach (var left in Items)
        {
            foreach (var right in other.Items)
            {
                counts.Add(new KeyValuePair<int, BigInteger>(left.Key - right.Key, left.Value * right.Value));
            }
        }

        return FromCounts(counts);
    }

    public Dist Negate()
    {
        var counts = Items
            .Select(item => new KeyValuePair<int, BigInteger>(-item.Key, item.Value));
        return FromCounts(counts);
    }

    public Dist Scale(int factor)
    {
        var counts = Items
            .Select(item => new KeyValuePair<int, BigInteger>(item.Key * factor, item.Value));
        return FromCounts(counts);
    }
}
