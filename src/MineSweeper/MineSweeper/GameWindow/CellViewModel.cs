using CommunityToolkit.Mvvm.ComponentModel;
using MineSweeper.Domain;

namespace MineSweeper.GameWindow;

public sealed class CellViewModel(Cell cell) : ObservableObject
{
    public Cell Cell { get; } = cell;

    public CellState State => Cell.State;

    public Position Position => Cell.Position;

    public int NeighborMinesCount => Cell.NeighborMinesCount;

    public bool IsMine => Cell.IsMine;

    public bool IsRevealed => Cell.IsRevealed;

    public bool IsFlagged => Cell.IsFlagged;

    public void Refresh()
    {
        Cell.Reveal();
        OnPropertyChanged(nameof(State));
        OnPropertyChanged(nameof(IsRevealed));
        OnPropertyChanged(nameof(IsFlagged));
        OnPropertyChanged(nameof(Cell));
    }
}