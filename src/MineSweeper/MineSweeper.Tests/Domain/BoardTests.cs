using MineSweeper.Domain;

namespace MineSweeper.Tests.Domain;

public sealed class BoardTests
{
    [Fact]
    public void CreateInstance_InitializesCellsCorrectly()
    {
        var board = Board.CreateInstance(3, 4);

        Assert.Equal(3, board.RowsCount);
        Assert.Equal(4, board.ColumnsCount);

        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 4; c++)
            Assert.Equal(new Position(r, c), board[r, c].Position);
    }

    [Fact]
    public void Indexer_ReturnsCorrectCell()
    {
        var board = Board.CreateInstance(2, 2);
        var pos = new Position(1, 1);
        var cell = board[pos];
        Assert.Equal(pos, cell.Position);
        Assert.Equal(cell, board[1, 1]);
    }

    [Fact]
    public void PlaceMine_SetsMineAndIncrementsNeighbors()
    {
        var board = Board.CreateInstance(3, 3);
        var pos = new Position(1, 1);

        board.PlaceMine(pos);

        Assert.True(board[pos].IsMine);

        foreach (var neighbor in board.GetNeighborCells(pos))
        {
            if (!neighbor.IsMine)
                Assert.Equal(1, neighbor.NeighborMinesCount);
        }
    }

    [Fact]
    public void PlaceMine_Throws_OnInvalidPosition()
    {
        var board = Board.CreateInstance(2, 2);
        Assert.Throws<ArgumentOutOfRangeException>(() => board.PlaceMine(new Position(-1, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => board.PlaceMine(new Position(0, 2)));
    }

    [Fact]
    public void GetCell_ReturnsCorrectCell()
    {
        var board = Board.CreateInstance(2, 2);
        var pos = new Position(1, 0);
        var cell = board.GetCell(pos);
        Assert.Equal(pos, cell.Position);
    }

    [Fact]
    public void GetCell_Throws_OnInvalidPosition()
    {
        var board = Board.CreateInstance(2, 2);
        Assert.Throws<ArgumentOutOfRangeException>(() => board.GetCell(new Position(2, 0)));
    }

    [Fact]
    public void GetNeighborCells_ReturnsCorrectNeighbors()
    {
        var board = Board.CreateInstance(3, 3);
        var pos = new Position(1, 1);
        var neighbors = board.GetNeighborCells(pos).ToList();
        Assert.Equal(8, neighbors.Count);
        Assert.DoesNotContain(board[pos], neighbors);
    }

    [Fact]
    public void GetNeighborCells_HandlesEdges()
    {
        var board = Board.CreateInstance(2, 2);
        var pos = new Position(0, 0);
        var neighbors = board.GetNeighborCells(pos).ToList();
        Assert.Equal(3, neighbors.Count);
    }

    [Fact]
    public void GetAdjacentMinesCount_ReturnsCorrectCount()
    {
        var board = Board.CreateInstance(3, 3);
        board.PlaceMine(new Position(0, 0));
        board.PlaceMine(new Position(0, 1));
        var count = board.GetAdjacentMinesCount(new Position(1, 1));
        Assert.Equal(2, count);
    }

    [Fact]
    public void GetUnrevealedCellsCount_ReturnsAllInitially()
    {
        var board = Board.CreateInstance(2, 2);
        Assert.Equal(4, board.GetUnrevealedCellsCount());
    }

    [Fact]
    public void RevealCell_RevealsCellAndAffectedCells()
    {
        var board = Board.CreateInstance(2, 2);
        var pos = new Position(0, 0);
        board.RevealCell(pos, out var affected);
        Assert.Contains(board[pos], affected);
        Assert.True(board[pos].IsRevealed);
    }

    [Fact]
    public void RevealCell_DoesNothing_IfAlreadyRevealedOrFlagged()
    {
        var board = Board.CreateInstance(2, 2);
        var pos = new Position(0, 0);
        board.RevealCell(pos, out _);
        board.RevealCell(pos, out var affected2);
        Assert.Empty(affected2);

        var pos2 = new Position(1, 1);
        board[pos2].ToggleFlag();
        board.RevealCell(pos2, out var affected3);
        Assert.Empty(affected3);
    }

    [Fact]
    public void AreAllCellsRevealedOrFlagged_ReturnsTrue_WhenAllRevealed()
    {
        var board = Board.CreateInstance(2, 2);
        foreach (var cell in board.GetAllCells())
            cell.Reveal();
        Assert.True(board.AreAllCellsRevealedOrFlagged());
    }

    [Fact]
    public void AreAllCellsRevealedOrFlagged_ReturnsFalse_WhenAnyHidden()
    {
        var board = Board.CreateInstance(2, 2);
        board[0, 0].Reveal();
        Assert.False(board.AreAllCellsRevealedOrFlagged());
    }

    [Fact]
    public void AreAllCellsRevealedOrFlagged_ReturnsFalse_WhenFlaggedNonMine()
    {
        var board = Board.CreateInstance(2, 2);
        board[0, 0].ToggleFlag();
        Assert.False(board.AreAllCellsRevealedOrFlagged());
    }

    [Fact]
    public void GetAllCells_ReturnsAllCells()
    {
        var board = Board.CreateInstance(2, 2);
        var all = board.GetAllCells().ToList();
        Assert.Equal(4, all.Count);
        Assert.All(all, Assert.NotNull);
        for (var r = 0; r < 2; r++)
        for (var c = 0; c < 2; c++)
        {
            Assert.Contains(all, cell => cell.Position == new Position(r, c));
            Assert.Contains(all, cell => cell == board[r, c]);
        }
    }

    [Fact]
    public void PlaceMines_PlacesCorrectNumberOfMines()
    {
        var board = Board.CreateInstance(3, 3);
        var random = new Random(42);
        board.PlaceMines(4, random);
        Assert.Equal(4, board.GetAllCells().Count(c => c.IsMine));
    }

    [Fact]
    public void PlaceMines_Throws_IfTooManyMines()
    {
        var board = Board.CreateInstance(2, 2);
        var random = new Random(1);
        Assert.Throws<ArgumentException>(() => board.PlaceMines(5, random));
    }

    [Fact]
    public void PlaceMines_DoesNothing_IfZero()
    {
        var board = Board.CreateInstance(2, 2);
        var random = new Random(1);
        board.PlaceMines(0, random);
        Assert.Equal(0, board.GetAllCells().Count(c => c.IsMine));
    }

    [Fact]
    public void PlaceMines_Throws_IfRandomNull()
    {
        var board = Board.CreateInstance(2, 2);
        Assert.Throws<ArgumentNullException>(() => board.PlaceMines(1, null!));
    }

    [Fact]
    public void RevealCell_RecursivelyRevealsEmptyCells()
    {
        var board = Board.CreateInstance(3, 3);
        board.PlaceMine(new Position(0, 0));
        board.RevealCell(new Position(2, 2), out var affected);
        Assert.All(affected, c => Assert.True(c.IsRevealed));
        Assert.Contains(board[2, 2], affected);
    }

    [Fact]
    public void RevealCell_StopsAtMines()
    {
        var board = Board.CreateInstance(2, 2);
        board.PlaceMine(new Position(0, 0));
        board.RevealCell(new Position(1, 1), out var affected);
        Assert.DoesNotContain(board[0, 0], affected);
    }

    [Fact]
    public void GetUnrevealedMinesCount_ReturnsCorrectCount()
    {
        var board = Board.CreateInstance(2, 2);
        board.PlaceMine(new Position(0, 0));
        board.PlaceMine(new Position(1, 1));
        Assert.Equal(2, board.GetUnrevealedMinesCount());
        board[0, 0].Reveal();
        Assert.Equal(1, board.GetUnrevealedMinesCount());
    }
}