using MineSweeper.Domain;

namespace MineSweeper.Tests.ViewModels;

internal sealed class MockCell : ICell
{
    public CellState State { get; set; } = CellState.Hidden;
    public Position Position { get; set; }
    public bool IsMine { get; set; }
    public sbyte NeighborMinesCount { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsQuestionMarked { get; set; }
    public bool IsExploded { get; set; }

    public bool RevealCalled { get; private set; }
    public bool ToggleFlagCalled { get; private set; }
    public bool PlaceMineCalled { get; private set; }
    public bool RevealMineCalled { get; private set; }
    public bool IncreaseNeighborMinesCountCalled { get; private set; }

    void ICell.Reveal()
    {
        RevealCalled = true;
    }

    void ICell.ToggleFlag()
    {
        ToggleFlagCalled = true;
    }

    void ICell.PlaceMine()
    {
        PlaceMineCalled = true;
    }

    void ICell.RevealMine()
    {
        RevealMineCalled = true;
    }

    void ICell.IncreaseNeighborMinesCount()
    {
        IncreaseNeighborMinesCountCalled = true;
    }
}