namespace Dieodds;

public static class Eval
{
    public static Dist Evaluate(Node node)
    {
        return node switch
        {
            Number number => Dist.Constant(number.Value),
            Dice dice => KeepSum.Of(
                Die.One(dice.Sides, dice.Mod, dice.RerollMax),
                dice.Count,
                dice.Keep,
                dice.Kept),
            Group group => Evaluate(group.Inner),
            Neg negation => Evaluate(negation.Operand).Negate(),
            Binary { Op: '+' } binary => Evaluate(binary.Left).Add(Evaluate(binary.Right)),
            Binary { Op: '-' } binary => Evaluate(binary.Left).Subtract(Evaluate(binary.Right)),
            Binary { Op: '*' } binary => EvaluateMultiplication(binary),
            _ => throw new ArgumentException("Unsupported expression node.", nameof(node))
        };
    }

    private static Dist EvaluateMultiplication(Binary binary)
    {
        if (binary.Right is Number rightMultiplier)
        {
            return Evaluate(binary.Left).Scale(rightMultiplier.Value);
        }

        var leftMultiplier = (Number)binary.Left;
        return Evaluate(binary.Right).Scale(leftMultiplier.Value);
    }
}
