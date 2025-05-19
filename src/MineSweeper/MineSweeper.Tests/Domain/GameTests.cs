using System.Reflection;
using MineSweeper.Domain;

namespace MineSweeper.Tests.Domain;

public sealed class GameTests
{
    [Fact]
    public void Create_InitializesGameWithCorrectStateAndMines()
    {
        var game = Game.Create(3, 3, 2, Shared.Random);
        Assert.Equal(GameState.Running, game.State);
        Assert.Equal(2, game.MinesCount);
        Assert.NotNull(game.Board);
    }

    [Fact]
    public void RevealCell_RevealsCellAndReturnsSuccess_WhenNotMine()
    {
        var game = Game.Create(2, 2, 0, Shared.Random);
        var pos = new Position(0, 0);
        var result = game.RevealCell(pos, out var affectedCells);
        Assert.True(result.IsSuccess);
        Assert.NotNull(affectedCells);
        Assert.Contains(affectedCells, c => c.Position.Equals(pos));
    }

    [Fact]
    public void RevealCell_RevealsCellAndReturnsSuccess_WithoutAffectedCells_WhenNotMine()
    {
        var game = Game.Create(2, 2, 0, Shared.Random);
        var pos = new Position(0, 0);
        var result = game.RevealCell(pos);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RevealCell_WinsGame_WhenAllNonMinesRevealed()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        // Find a non-mine cell
        Position? safePos = null;
        foreach (var cell in game.Board.GetAllCells())
        {
            if (!cell.IsMine)
            {
                safePos = cell.Position;
                break;
            }
        }

        Assert.NotNull(safePos);
        // Reveal all non-mine cells
        foreach (var cell in game.Board.GetAllCells())
        {
            if (!cell.IsMine)
                game.RevealCell(cell.Position);
            else
                game.ToggleFlag(cell.Position);
        }

        Assert.True(game.Board.AreAllCellsRevealedOrFlagged());

        Assert.Equal(GameState.Won, game.State);
    }

    [Fact]
    public void ToggleFlag_TogglesFlagAndReturnsSuccess()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        var pos = new Position(0, 0);
        var result = game.ToggleFlag(pos);
        Assert.True(result.IsSuccess);
        var cell = game.Board.GetCell(pos);
        Assert.True(cell.IsFlagged);
    }

    [Fact]
    public void ToggleFlag_WinsGame_WhenAllCellsRevealedOrFlagged()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        // Flag all mine cells, reveal all others
        foreach (var cell in game.Board.GetAllCells())
        {
            if (cell.IsMine)
                game.ToggleFlag(cell.Position);
            else
                game.RevealCell(cell.Position);
        }

        Assert.Equal(GameState.Won, game.State);
    }

    [Fact]
    public void FullGame_SimulatesGameFromStartToWin()
    {
        // Arrange: 3x3 board, 2 mines
        var game = Game.Create(3, 3, 2, Shared.Random);

        // Find all mine and non-mine positions
        List<Position> minePositions = [];
        var safePositions = new List<Position>();
        foreach (var cell in game.Board.GetAllCells())
        {
            if (cell.IsMine)
                minePositions.Add(cell.Position);
            else
                safePositions.Add(cell.Position);
        }

        // Act: Flag all mines
        foreach (var pos in minePositions)
        {
            var flagResult = game.ToggleFlag(pos);
            Assert.True(flagResult.IsSuccess || flagResult == OperationResults.GameWon);
        }

        // Reveal all safe cells
        foreach (var pos in safePositions)
        {
            if (game.Board[pos].IsRevealed)
                continue;

            var revealResult = game.RevealCell(pos, out var affectedCells);
            Assert.True(revealResult.IsSuccess || revealResult == OperationResults.GameWon);
            Assert.NotNull(affectedCells);
            Assert.Contains(affectedCells, c => c.Position.Equals(pos));
        }

        // Assert: Game is won
        Assert.Equal(GameState.Won, game.State);
    }

    [Fact]
    public void FullGame_SimulatesGameFromStartToLose()
    {
        // Arrange: 3x3 board, 2 mines
        var game = Game.Create(3, 3, 2, Shared.Random);

        // Find a mine position
        Position? minePos = null;
        foreach (var cell in game.Board.GetAllCells())
        {
            if (!cell.IsMine)
                continue;

            minePos = cell.Position;
            break;
        }

        Assert.NotNull(minePos);

        // Act: Reveal a mine cell
        var result = game.RevealCell(minePos.Value, out var affectedCells);

        // Assert: Game is over
        Assert.Equal(GameState.GameOver, game.State);
        Assert.Equal(OperationResults.GameOver, result);
        Assert.NotNull(affectedCells);
    }

    [Fact]
    public void RevealObviousNeighborCells_RevealsUnflaggedUnrevealedNeighbors_WhenFlagCountMatches()
    {
        // Arrange: 3x3 board, 1 mine
        var game = Game.Create(3, 3, 1, Shared.Random);

        // Find a cell with at least one neighbor mine
        var targetCell = game.Board.GetAllCells().FirstOrDefault(cell =>
            !cell.IsMine && game.Board.GetNeighborCells(cell.Position).Any(n => n.IsMine));

        Assert.NotNull(targetCell);

        // Flag all neighboring mines
        foreach (var neighbor in game.Board.GetNeighborCells(targetCell.Position))
        {
            if (neighbor.IsMine)
                game.ToggleFlag(neighbor.Position);
        }

        // Reveal the target cell
        game.RevealCell(targetCell.Position);

        // Act: Reveal obvious neighbors
        game.RevealObviousNeighborCells(targetCell.Position, out var affectedCells);

        // Assert: All unflagged, unrevealed neighbors are revealed
        foreach (var neighbor in game.Board.GetNeighborCells(targetCell.Position))
        {
            if (neighbor is { IsMine: false, IsFlagged: false })
                Assert.Contains(affectedCells, c => c.Position.Equals(neighbor.Position));
        }
    }

    [Fact]
    public void Create_InitializesBoardWithCorrectDimensions()
    {
        var game = Game.Create(4, 5, 3, Shared.Random);
        Assert.Equal(4, game.Board.RowsCount);
        Assert.Equal(5, game.Board.ColumnsCount);
    }

    [Fact]
    public void RevealCell_ReturnsGameOver_WhenMineIsRevealed()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        // Find a mine cell
        var mineCell = game.Board.GetAllCells().FirstOrDefault(c => c.IsMine);
        Assert.NotNull(mineCell);
        var result = game.RevealCell(mineCell.Position, out var affectedCells);
        Assert.Equal(OperationResults.GameOver, result);
        Assert.Equal(GameState.GameOver, game.State);
        Assert.NotNull(affectedCells);
    }

    [Fact]
    public void RevealObviousNeighborCells_DoesNothing_WhenCellNotRevealedOrNoNeighborMines()
    {
        var game = Game.Create(3, 3, 1, Shared.Random);
        var cell = game.Board.GetAllCells().First(c => !c.IsMine && c.NeighborMinesCount == 0);
        // Not revealed yet
        var result = game.RevealObviousNeighborCells(cell.Position, out var affectedCells);
        Assert.True(result.IsSuccess);
        Assert.Empty(affectedCells);

        // Reveal cell, still no neighbor mines
        game.RevealCell(cell.Position);
        result = game.RevealObviousNeighborCells(cell.Position, out affectedCells);
        Assert.True(result.IsSuccess);
        Assert.Empty(affectedCells);
    }

    [Fact]
    public void RevealObviousNeighborCells_GameOver_WhenRevealedNeighborIsMine()
    {
        var game = Game.Create(3, 3, [(row: 1, column: 1)]);

        game.RevealCell((row: 2, column: 2));
        game.ToggleFlag((row: 1, column: 2));

        var result = game.RevealObviousNeighborCells((row: 2, column: 2), out var affectedCells);
        Assert.Equal(OperationResults.GameOver, result);
        Assert.Equal(GameState.GameOver, game.State);
        Assert.NotNull(affectedCells);
    }

    [Fact]
    public void ToggleFlag_DoesNotWinGame_IfNotAllCellsRevealedOrFlagged()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        var pos = game.Board.GetAllCells().First(c => c.IsMine).Position;
        var result = game.ToggleFlag(pos);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(GameState.Won, game.State);
    }

    [Fact]
    public void ToggleFlag_TogglesFlagOff()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        var pos = game.Board.GetAllCells().First().Position;
        game.ToggleFlag(pos);
        var cell = game.Board.GetCell(pos);
        Assert.True(cell.IsFlagged);
        game.ToggleFlag(pos);
        Assert.False(cell.IsFlagged);
    }

    [Fact]
    public void Start_ThrowsIfGameAlreadyStarted()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        var startMethod = typeof(Game).GetMethod("Start",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(startMethod);
        var ex = Assert.Throws<TargetInvocationException>(() => startMethod.Invoke(game, null));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public void Create_WithMinePositions_InitializesGameWithCorrectMineCount()
    {
        Position[] minePositions = [(0, 0), new(1, 1)];
        var game = Game.Create(3, 3, minePositions);
        Assert.Equal(GameState.Running, game.State);
        Assert.Equal(minePositions.Length, game.MinesCount);
        foreach (var pos in minePositions)
        {
            Assert.True(game.Board[pos].IsMine);
        }
    }

    [Fact]
    public void RevealCell_ReturnsGameOver_WhenRevealedCellIsMine()
    {
        Position[] minePositions = [(0, 0), (3, 3)];
        var game = Game.Create(4, 4, minePositions);
        var result = game.RevealCell((0, 0), out var affectedCells);
        Assert.Equal(OperationResults.GameOver, result);
        Assert.Equal(GameState.GameOver, game.State);
        Assert.NotNull(affectedCells);
        Assert.Contains(affectedCells, c => c.Position.Equals((3, 3)));
    }

    [Fact]
    public void RevealCell_ReturnsGameWon_WhenAllCellsRevealedOrFlagged()
    {
        Position[] minePositions = [new(0, 0)];
        var game = Game.Create(2, 2, minePositions);
        // Flag the mine
        game.ToggleFlag(new Position(0, 0));
        // Reveal all other cells
        foreach (var cell in game.Board.GetAllCells())
        {
            if (!cell.IsMine)
                game.RevealCell(cell.Position);
        }

        Assert.Equal(GameState.Won, game.State);
    }

    [Fact]
    public void ToggleFlag_ReturnsGameWon_WhenAllCellsRevealedOrFlagged()
    {
        Position[] minePositions = [new(0, 0)];
        var game = Game.Create(2, 2, minePositions);
        // Reveal all non-mine cells
        foreach (var cell in game.Board.GetAllCells())
        {
            if (!cell.IsMine)
                game.RevealCell(cell.Position);
        }

        // Flag the mine
        var result = game.ToggleFlag(new Position(0, 0));
        Assert.Equal(OperationResults.GameWon, result);
        Assert.Equal(GameState.Won, game.State);
    }

    [Fact]
    public void RevealObviousNeighborCells_DoesNothing_IfCellNotRevealedOrNoNeighborMines()
    {
        var game = Game.Create(3, 3, 0, Shared.Random);
        var cell = game.Board.GetAllCells().First();
        var result = game.RevealObviousNeighborCells(cell.Position, out var affectedCells);
        Assert.True(result.IsSuccess);
        Assert.Empty(affectedCells);
    }

    [Fact]
    public void Start_ThrowsInvalidOperationException_IfGameAlreadyStarted()
    {
        var game = Game.Create(2, 2, 1, Shared.Random);
        var startMethod = typeof(Game).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(startMethod);
        var ex = Assert.Throws<TargetInvocationException>(() => startMethod.Invoke(game, null));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }
}
