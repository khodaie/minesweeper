using MineSweeper.Domain;

namespace MineSweeper.Tests.Domain;

public sealed class GameTests
{
    [Fact]
    public void Create_InitializesGameWithCorrectStateAndMines()
    {
        var game = Game.Create(3, 3, 2, Shared.Random);
        Assert.Equal(GameState.InProgress, game.State);
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
        var minePositions = new List<Position>();
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
            if (cell.IsMine)
            {
                minePos = cell.Position;
                break;
            }
        }

        Assert.NotNull(minePos);

        // Act: Reveal a mine cell
        var result = game.RevealCell(minePos.Value, out var affectedCells);

        // Assert: Game is over
        Assert.Equal(GameState.GameOver, game.State);
        Assert.Equal(OperationResults.GameOver, result);
        Assert.NotNull(affectedCells);
        Assert.Contains(affectedCells, c => c.Position.Equals(minePos.Value));
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
}
