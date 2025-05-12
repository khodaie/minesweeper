using System.Diagnostics;

namespace MineSweeper.Domain;

[DebuggerDisplay("Cell[{Row}, {Column}]: {State}")]
public sealed class Cell
{
    public CellState State { get; private set; } = CellState.Hidden;

    public Position Position { get; }

    public bool IsMine { get; private set; }

    public int NeighborMinesCount { get; private set; }

    public bool IsRevealed => State == CellState.Revealed;

    public bool IsFlagged => State == CellState.Flagged;

    private Cell(in Position position)
    {
        Position = position;
    }

    internal static Cell CreateInstance(int row, int column) => new(new Position(row, column));

    internal void PlaceMine()
    {
        if(State is not CellState.Hidden)
            throw new InvalidOperationException("Cannot place a mine on a revealed or flagged cell.");

        IsMine = true;
        NeighborMinesCount = -1;
    }

    internal void IncreaseNeighborMinesCount()
    {
        if (State is not CellState.Hidden)
            throw new InvalidOperationException("Cannot set neighbor mines count on a revealed or flagged cell.");
                
        NeighborMinesCount++;
    }

    public void Reveal()
    {
        if(IsRevealed || IsFlagged)
            return;

        State = CellState.Revealed;
    }

    public void ToggleFlag()
    {
        if (IsRevealed)
            return;

        State = State == CellState.Flagged ? CellState.Hidden : CellState.Flagged;
    }    
}
