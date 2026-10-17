using System.Globalization;

namespace Dieodds;

public abstract record Node;

public record Number(int Value) : Node;

public enum Mod
{
    None,
    Explode,
    Reroll
}

public enum KeepKind
{
    None,
    High,
    Low
}

public record Dice(int Count, int Sides, Mod Mod, int RerollMax, KeepKind Keep, int Kept) : Node;

public record Group(Node Inner) : Node;

public record Neg(Node Operand) : Node;

public record Binary(char Op, Node Left, Node Right) : Node;

public static class Parser
{
    public static Node Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
        {
            throw new DieException("empty expression");
        }

        if (text.Length > 200)
        {
            throw new DieException("expression is longer than 200 characters");
        }

        var reader = new Reader(text);
        var result = reader.ParseExpression();
        reader.SkipSeparators();

        if (!reader.AtEnd)
        {
            reader.ThrowUnexpected();
        }

        return result;
    }

    private sealed class Reader
    {
        private readonly string text;
        private int _position;

        public Reader(string source)
        {
            text = source;
        }
        private int _diceTerms;

        public bool AtEnd => _position >= text.Length;

        public Node ParseExpression()
        {
            SkipSeparators();
            var negateFirstTerm = TryConsume('-');
            var firstTerm = ParseTerm();
            Node result = negateFirstTerm ? new Neg(firstTerm) : firstTerm;

            while (true)
            {
                SkipSeparators();
                if (!TryPeek(out var op) || op is not ('+' or '-'))
                {
                    return result;
                }

                _position++;
                var right = ParseTerm();
                result = new Binary(op, result, right);
            }
        }

        public Node ParseTerm()
        {
            var left = ParseFactor();

            while (true)
            {
                SkipSeparators();
                if (!TryConsume('*'))
                {
                    return left;
                }

                var starPosition = _position - 1;
                SkipSeparators();
                var right = ParseFactor();

                if (left is not Number && right is not Number)
                {
                    throw new DieException(
                        $"* needs a number on one side at column {FormatColumn(starPosition)}");
                }

                left = new Binary('*', left, right);
            }
        }

        private Node ParseFactor()
        {
            SkipSeparators();
            if (!TryPeek(out var character))
            {
                throw new DieException("unexpected end of the expression");
            }

            if (character == '(')
            {
                _position++;
                var inner = ParseExpression();
                SkipSeparators();

                if (AtEnd)
                {
                    throw new DieException("unexpected end of the expression");
                }

                if (text[_position] != ')')
                {
                    ThrowUnexpected();
                }

                _position++;
                return new Group(inner);
            }

            if (character == 'd')
            {
                return ParseDice(_position, null);
            }

            if (IsAsciiDigit(character))
            {
                var start = _position;
                var digits = ReadDigits();
                if (!AtEnd && text[_position] == 'd')
                {
                    return ParseDice(start, digits);
                }

                if (!TryParseNumber(digits, out var value))
                {
                    throw new DieException(
                        $"invalid number {Text.Show(digits)} at column {FormatColumn(start)}");
                }

                return new Number(value);
            }

            ThrowUnexpected();
            throw new InvalidOperationException();
        }

        private Dice ParseDice(int start, string? countDigits)
        {
            var count = countDigits is null ? 1 : ParseDiceInteger(countDigits);
            _position++;

            if (AtEnd)
            {
                throw new DieException("unexpected end of the expression");
            }

            var sidesAreNumbered = false;
            var sides = 0;
            if (IsAsciiDigit(text[_position]))
            {
                sidesAreNumbered = true;
                sides = ParseDiceInteger(ReadDigits());
            }
            else if (TryConsume('%'))
            {
                sidesAreNumbered = true;
                sides = 100;
            }
            else if (TryConsume('F'))
            {
                sides = 0;
            }
            else
            {
                ThrowUnexpected();
            }

            var modifier = Mod.None;
            var rerollMax = 0;
            if (TryConsume('!'))
            {
                modifier = Mod.Explode;
            }
            else if (TryConsume('r'))
            {
                modifier = Mod.Reroll;
                if (AtEnd || !IsAsciiDigit(text[_position]))
                {
                    if (AtEnd)
                    {
                        throw new DieException("unexpected end of the expression");
                    }

                    ThrowUnexpected();
                }

                rerollMax = ParseDiceInteger(ReadDigits());
            }

            var keep = KeepKind.None;
            var kept = count;
            if (!AtEnd && text[_position] == 'k')
            {
                _position++;
                if (AtEnd)
                {
                    throw new DieException("unexpected end of the expression");
                }

                if (text[_position] == 'h')
                {
                    keep = KeepKind.High;
                }
                else if (text[_position] == 'l')
                {
                    keep = KeepKind.Low;
                }
                else
                {
                    ThrowUnexpected();
                }

                _position++;
                kept = 1;
                if (!AtEnd && IsAsciiDigit(text[_position]))
                {
                    kept = ParseDiceInteger(ReadDigits());
                }
            }

            if (!AtEnd && text[_position] is '!' or 'r' or 'k')
            {
                ThrowUnexpected();
            }

            _diceTerms++;
            var item = text[start.._position];
            var hasModifier = modifier != Mod.None || keep != KeepKind.None;
            var reason = GetDiceError(count, sides, sidesAreNumbered, modifier, rerollMax, keep, kept, hasModifier);
            if (reason is null && _diceTerms > 3)
            {
                reason = "at most 3 dice terms";
            }

            if (reason is not null)
            {
                throw new DieException(
                    $"invalid dice {Text.Show(item)} at column {FormatColumn(start)} ({reason})");
            }

            return new Dice(count, sides, modifier, rerollMax, keep, kept);
        }

        private string? GetDiceError(
            int count,
            int sides,
            bool sidesAreNumbered,
            Mod modifier,
            int rerollMax,
            KeepKind keep,
            int kept,
            bool hasModifier)
        {
            var maxDice = hasModifier ? 10 : 20;
            if (count < 1 || count > maxDice)
            {
                return hasModifier ? "1 to 10 dice with a modifier" : "1 to 20 dice";
            }

            if (sidesAreNumbered)
            {
                var maxSides = hasModifier ? 20 : 100;
                if (sides < 2 || sides > maxSides)
                {
                    return hasModifier ? "2 to 20 sides with a modifier" : "2 to 100 sides";
                }
            }

            if (!sidesAreNumbered && modifier != Mod.None)
            {
                return "! and r need numbered sides";
            }

            if (modifier == Mod.Reroll && (rerollMax < 1 || rerollMax > sides - 1))
            {
                return $"a reroll from 1 to {FormatInteger(sides - 1)}";
            }

            if (keep != KeepKind.None && (kept < 1 || kept > count))
            {
                return $"kept dice from 1 to {FormatInteger(count)}";
            }

            return null;
        }

        private string ReadDigits()
        {
            var start = _position;
            while (!AtEnd && IsAsciiDigit(text[_position]))
            {
                _position++;
            }

            return text[start.._position];
        }

        private static bool TryParseNumber(string digits, out int value)
        {
            value = 0;
            if (digits.Length > 4 || (digits.Length > 1 && digits[0] == '0'))
            {
                return false;
            }

            foreach (var digit in digits)
            {
                value = (value * 10) + (digit - '0');
            }

            return true;
        }

        private static int ParseDiceInteger(string digits)
        {
            if (digits.Length > 6 || (digits.Length > 1 && digits[0] == '0'))
            {
                return 0;
            }

            var value = 0;
            foreach (var digit in digits)
            {
                value = (value * 10) + (digit - '0');
            }

            return value;
        }

        public void SkipSeparators()
        {
            while (!AtEnd && text[_position] is ' ' or '\t')
            {
                _position++;
            }
        }

        private bool TryConsume(char expected)
        {
            if (AtEnd || text[_position] != expected)
            {
                return false;
            }

            _position++;
            return true;
        }

        private bool TryPeek(out char character)
        {
            if (AtEnd)
            {
                character = default;
                return false;
            }

            character = text[_position];
            return true;
        }

        public void ThrowUnexpected()
        {
            var character = text[_position].ToString();
            throw new DieException(
                $"unexpected {Text.Show(character)} at column {FormatColumn(_position)}");
        }

        private static bool IsAsciiDigit(char character)
        {
            return character is >= '0' and <= '9';
        }
    }

    private static string FormatColumn(int zeroBased)
    {
        return FormatInteger(zeroBased + 1);
    }

    private static string FormatInteger(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
