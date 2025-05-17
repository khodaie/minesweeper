using MineSweeper.Domain;

namespace MineSweeper.Tests.Domain;

public sealed class CellTests
{
    [Fact]
    public void CreateInstance_CheckPosition()
    {
        // Act
        var cell = Cell.CreateInstance(47, 37);

        // Assert
        Assert.Equal(cell.Position, new Position(47, 37));
    }

    [Fact]
    public void CreateInstanceWithPosition_CheckPosition()
    {
        // Act
        var position = new Position(47, 37);

        var cell = Cell.CreateInstance(position);

        // Assert
        Assert.Equal(cell.Position, position);
    }

    [Fact]
    public void PlaceMine_SetsIsMineToTrue()
    {
        // Arrange
        var cell = Cell.CreateInstance(1, 1);

        // Act
        cell.PlaceMine();

        // Assert
        Assert.True(cell.IsMine);
    }

    [Fact]
    public void Reveal_RevealsCell_WhenNotFlaggedOrRevealed()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.Reveal();
        Assert.True(cell.IsRevealed);
        Assert.Equal(CellState.Revealed, cell.State);
    }

    [Fact]
    public void Reveal_DoesNotReveal_WhenFlagged()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.ToggleFlag();
        cell.Reveal();
        Assert.False(cell.IsRevealed);
        Assert.Equal(CellState.Flagged, cell.State);
    }

    [Fact]
    public void Reveal_SetsStateToIsExploded_WhenMine()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.PlaceMine();
        cell.Reveal();
        Assert.True(cell.IsExploded);
        Assert.Equal(CellState.IsExploded, cell.State);
    }

    [Fact]
    public void ToggleFlag_CyclesThroughFlaggedQuestionMarkedHidden()
    {
        var cell = Cell.CreateInstance(0, 0);
        Assert.Equal(CellState.Hidden, cell.State);

        cell.ToggleFlag();
        Assert.True(cell.IsFlagged);
        Assert.Equal(CellState.Flagged, cell.State);

        cell.ToggleFlag();
        Assert.True(cell.IsQuestionMarked);
        Assert.Equal(CellState.QuestionMarked, cell.State);

        cell.ToggleFlag();
        Assert.Equal(CellState.Hidden, cell.State);
    }

    [Fact]
    public void ToggleFlag_DoesNothing_WhenRevealed()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.Reveal();
        var prevState = cell.State;
        cell.ToggleFlag();
        Assert.Equal(prevState, cell.State);
    }

    [Fact]
    public void PlaceMine_Throws_WhenNotHidden()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.Reveal();
        Assert.Throws<InvalidOperationException>(() => cell.PlaceMine());
    }

    [Fact]
    public void RevealMine_SetsStateToMineRevealed_WhenMineAndNotRevealed()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.PlaceMine();
        cell.RevealMine();
        Assert.Equal(CellState.MineRevealed, cell.State);
    }

    [Fact]
    public void RevealMine_DoesNothing_IfNotMineOrAlreadyRevealed()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.RevealMine();
        Assert.NotEqual(CellState.MineRevealed, cell.State);

        var mineCell = Cell.CreateInstance(1, 1);
        mineCell.PlaceMine();
        mineCell.Reveal();
        mineCell.RevealMine();
        Assert.NotEqual(CellState.MineRevealed, mineCell.State);
    }

    [Fact]
    public void IncreaseNeighborMinesCount_Increments_WhenHidden()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.IncreaseNeighborMinesCount();
        Assert.Equal(1, cell.NeighborMinesCount);
        cell.IncreaseNeighborMinesCount();
        Assert.Equal(2, cell.NeighborMinesCount);
    }

    [Fact]
    public void IncreaseNeighborMinesCount_Throws_WhenNotHidden()
    {
        var cell = Cell.CreateInstance(0, 0);
        cell.Reveal();
    }

    [Fact]
    public void IncreaseNeighborMinesCount_Throws_WhenInvalidValue()
    {
        var cell = Cell.CreateInstance(0, 0);

        for (var i = 0; i < 8; i++)
            cell.IncreaseNeighborMinesCount();

        Assert.Throws<InvalidOperationException>(() => cell.IncreaseNeighborMinesCount());
    }

    [Fact]
    public void IncreaseNeighborMinesCount_Throws_WhenIsMine()
    {
        var cell = Cell.CreateInstance(0, 0);

        cell.PlaceMine();

        Assert.Throws<InvalidOperationException>(() => cell.IncreaseNeighborMinesCount());
    }
}