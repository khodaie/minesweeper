using MineSweeper.Domain;

namespace MineSweeper.Tests.Domain;

public sealed class PositionTests
{
    [Fact]
    public void Constructor_SetsRowAndColumn()
    {
        var pos = new Position(2, 3);
        Assert.Equal(2, pos.Row);
        Assert.Equal(3, pos.Column);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(-5, -5)]
    public void Constructor_ThrowsIfNegative(int row, int column)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(row, column));
    }

    [Fact]
    public void Validate_DoesNotThrow_WhenWithinBounds()
    {
        var pos = new Position(1, 2);
        pos.Validate(3, 4); // Should not throw
    }

    [Theory]
    [InlineData(3, 0, 3, 4)] // Row == rows
    [InlineData(0, 4, 3, 4)] // Column == columns
    [InlineData(5, 2, 5, 3)] // Row == rows
    [InlineData(2, 3, 3, 3)] // Column == columns
    public void Validate_ThrowsIfOutOfBounds(int row, int column, int rows, int columns)
    {
        var pos = new Position(row, column);
        Assert.Throws<ArgumentOutOfRangeException>(() => pos.Validate(rows, columns));
    }

    [Fact]
    public void ImplicitOperator_CreatesPositionFromTuple()
    {
        Position pos = (4, 5);
        Assert.Equal(4, pos.Row);
        Assert.Equal(5, pos.Column);
    }

    [Theory]
    [MemberData(nameof(NeighborPositionsData))]
    public void GetNeighborPositions_ReturnsCorrectNeighbors(
        int row, int col, int rows, int cols, (int Row, int Column)[] expected)
    {
        var pos = new Position(row, col);
        var neighbors = pos.GetNeighborPositions(rows, cols).ToArray();

        var expectedPositions = expected.Select(t => new Position(t.Row, t.Column)).ToArray();

        Assert.Equal(expectedPositions.OrderBy(p => (p.Row, p.Column)), neighbors.OrderBy(p => (p.Row, p.Column)));
    }

    [Fact]
    public void GetNeighborPositions_ReturnsEmpty_WhenGridIs1x1()
    {
        var pos = new Position(0, 0);
        var neighbors = pos.GetNeighborPositions(1, 1).ToArray();
        Assert.Empty(neighbors);
    }

    public static TheoryData<int, int, int, int, (int Row, int Column)[]> NeighborPositionsData => new()
    {
        { 1, 1, 3, 3, [(0, 0), (0, 1), (0, 2), (1, 0), (1, 2), (2, 0), (2, 1), (2, 2)] },
        { 0, 0, 3, 3, [(0, 1), (1, 0), (1, 1)] },
        { 2, 2, 3, 3, [(1, 1), (1, 2), (2, 1)] },
        { 0, 2, 3, 3, [(0, 1), (1, 1), (1, 2)] },
        { 2, 0, 3, 3, [(1, 0), (1, 1), (2, 1)] },
        { 0, 1, 2, 3, [(0, 0), (0, 2), (1, 0), (1, 1), (1, 2)] }
    };
}