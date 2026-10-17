using System.Numerics;

namespace Dieodds;

public static class KeepSum
{
    public static Dist Of(Dist die, int count, KeepKind keep, int kept)
    {
        if (keep == KeepKind.None)
        {
            var sum = Dist.Constant(0);
            for (var index = 0; index < count; index++)
            {
                sum = sum.Add(die);
            }

            return sum;
        }

        // Processing faces in keep order means assigned dice always rank ahead of
        // unassigned dice. Choosing j of the remaining labeled dice contributes
        // C(remaining, j) times the face weight to the jth power; merging these
        // states counts every independent sequence without enumerating sequences.
        var faces = keep == KeepKind.High
            ? die.Items.Reverse()
            : die.Items;
        var choose = BinomialCoefficients(count);
        var states = new Dictionary<(int Assigned, int Sum), BigInteger>
        {
            [(0, 0)] = BigInteger.One
        };

        foreach (var face in faces)
        {
            var facePowers = new BigInteger[count + 1];
            facePowers[0] = BigInteger.One;
            for (var taken = 1; taken <= count; taken++)
            {
                facePowers[taken] = facePowers[taken - 1] * face.Value;
            }

            var nextStates = new Dictionary<(int Assigned, int Sum), BigInteger>();
            foreach (var state in states)
            {
                var remaining = count - state.Key.Assigned;
                for (var taken = 0; taken <= remaining; taken++)
                {
                    var assignments = choose[remaining, taken] * facePowers[taken];
                    var newlyKept = Math.Min(taken, Math.Max(0, kept - state.Key.Assigned));
                    var key = (
                        Assigned: state.Key.Assigned + taken,
                        Sum: state.Key.Sum + (face.Key * newlyKept));
                    var weight = state.Value * assignments;

                    if (nextStates.TryGetValue(key, out var currentWeight))
                    {
                        nextStates[key] = currentWeight + weight;
                    }
                    else
                    {
                        nextStates.Add(key, weight);
                    }
                }
            }

            states = nextStates;
        }

        return Dist.FromCounts(states
            .Where(state => state.Key.Assigned == count)
            .Select(state => new KeyValuePair<int, BigInteger>(state.Key.Sum, state.Value)));
    }

    private static BigInteger[,] BinomialCoefficients(int count)
    {
        var coefficients = new BigInteger[count + 1, count + 1];
        for (var n = 0; n <= count; n++)
        {
            coefficients[n, 0] = BigInteger.One;
            coefficients[n, n] = BigInteger.One;
            for (var k = 1; k < n; k++)
            {
                coefficients[n, k] = coefficients[n - 1, k - 1] + coefficients[n - 1, k];
            }
        }

        return coefficients;
    }
}
