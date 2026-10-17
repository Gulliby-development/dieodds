namespace Dieodds;

public static class Text
{
    public static string Show(string text)
    {
        var shown = new char[text.Length + 2];
        shown[0] = '"';

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            shown[index + 1] = character is >= '!' and <= '~' ? character : '?';
        }

        shown[^1] = '"';
        return new string(shown);
    }

    public static string ListOr(IReadOnlyList<string> items)
    {
        return items.Count switch
        {
            0 => string.Empty,
            1 => items[0],
            2 => $"{items[0]} or {items[1]}",
            _ => string.Join(", ", items.Take(items.Count - 1)) + $" or {items[^1]}"
        };
    }

    public static List<string> RenderTable(
        IReadOnlyList<string> header,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var widths = new int[header.Count];

        for (var column = 0; column < header.Count; column++)
        {
            widths[column] = header[column].Length;
        }

        foreach (var row in rows)
        {
            for (var column = 0; column < header.Count; column++)
            {
                widths[column] = Math.Max(widths[column], row[column].Length);
            }
        }

        var lines = new List<string>(rows.Count + 1)
        {
            RenderRow(header, widths)
        };

        foreach (var row in rows)
        {
            lines.Add(RenderRow(row, widths));
        }

        return lines;
    }

    private static string RenderRow(IReadOnlyList<string> cells, IReadOnlyList<int> widths)
    {
        var rendered = new string[cells.Count];
        for (var column = 0; column < cells.Count; column++)
        {
            rendered[column] = cells[column].PadRight(widths[column]);
        }

        return string.Join("  ", rendered).TrimEnd();
    }
}
