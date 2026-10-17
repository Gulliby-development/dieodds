namespace Dieodds.Tests;

public class UnitTest1
{
    [Fact]
    public void DieExceptionPreservesMessageAndDerivesFromException()
    {
        var exception = new DieException("failure");

        Assert.IsAssignableFrom<Exception>(exception);
        Assert.Equal("failure", exception.Message);
    }

    [Fact]
    public void ShowEnclosesEmptyText()
    {
        Assert.Equal("\"\"", Text.Show(string.Empty));
    }

    [Fact]
    public void ShowReplacesSpacesAndControlCharactersPerCodeUnit()
    {
        Assert.Equal("\"A?B??\"", Text.Show("A B\t\n"));
    }

    [Fact]
    public void ShowKeepsPrintableRangeBoundaries()
    {
        Assert.Equal("\"!~\"", Text.Show("!~"));
    }

    [Fact]
    public void ShowReplacesDeleteCharacter()
    {
        Assert.Equal("\"?\"", Text.Show("\u007f"));
    }

    [Fact]
    public void ShowReplacesBothCodeUnitsOfSurrogatePair()
    {
        Assert.Equal("\"??\"", Text.Show("\ud83d\ude00"));
    }

    [Fact]
    public void ShowReplacesIsolatedSurrogate()
    {
        Assert.Equal("\"?\"", Text.Show("\ud800"));
    }

    [Fact]
    public void ShowLeavesPrintableQuoteAndBackslashUnchanged()
    {
        Assert.Equal("\"\"\\\"", Text.Show("\"\\"));
    }

    [Fact]
    public void ListOrReturnsSingleItemUnchanged()
    {
        Assert.Equal("A", Text.ListOr(new[] { "A" }));
    }

    [Fact]
    public void ListOrJoinsTwoItemsWithOr()
    {
        Assert.Equal("A or B", Text.ListOr(new[] { "A", "B" }));
    }

    [Fact]
    public void ListOrUsesCommaAndOrForThreeItems()
    {
        Assert.Equal("A, B or C", Text.ListOr(new[] { "A", "B", "C" }));
    }

    [Fact]
    public void ListOrUsesCommaSeparatedItemsBeforeFinalOr()
    {
        Assert.Equal("A, B, C or D", Text.ListOr(new[] { "A", "B", "C", "D" }));
    }

    [Fact]
    public void RenderTablePadsColumnsToTheWidestCell()
    {
        IReadOnlyList<IReadOnlyList<string>> rows =
        [
            new[] { "wide", "x" },
            new[] { "b", "yy" }
        ];

        Assert.Equal(
            new[] { "a     long", "wide  x", "b     yy" },
            Text.RenderTable(new[] { "a", "long" }, rows));
    }

    [Fact]
    public void RenderTableReturnsHeaderWhenThereAreNoRows()
    {
        Assert.Equal(
            new[] { "name  n" },
            Text.RenderTable(new[] { "name", "n" }, Array.Empty<IReadOnlyList<string>>()));
    }

    [Fact]
    public void RenderTableTrimsTrailingSpacesAndReturnsLinesWithoutLineFeeds()
    {
        IReadOnlyList<IReadOnlyList<string>> rows =
        [
            new[] { "x", string.Empty }
        ];

        var lines = Text.RenderTable(new[] { "a", "b" }, rows);

        Assert.Equal(new[] { "a  b", "x" }, lines);
        Assert.DoesNotContain('\n', lines[0]);
        Assert.DoesNotContain('\r', lines[0]);
        Assert.DoesNotContain(' ', lines[1][^1..]);
    }
}
