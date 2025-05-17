using System.Diagnostics;

namespace MineSweeper.Domain;

[DebuggerDisplay("Cell[{Position.Row}, {Position.Column}]: {State}")]
public sealed class Cell
{
    public CellState State { get; private set; } = CellState.Hidden;

    public Position Position { get; }

    public bool IsMine { get; private set; }

    public int NeighborMinesCount { get; private set; }

    public bool IsRevealed => State is CellState.Revealed or CellState.IsExploded or CellState.MineRevealed;

    public bool IsFlagged => State == CellState.Flagged;

    public bool IsQuestionMarked => State == CellState.QuestionMarked;

    public bool IsExploded => State == CellState.IsExploded;

    private Cell(in Position position)
    {
        Position = position;
    }

    internal static Cell CreateInstance(int row, int column) => new(new Position(row, column));

    internal void Reveal()
    {
        if (IsRevealed || IsFlagged)
            return;

        State = IsMine ? CellState.IsExploded : CellState.Revealed;
    }

    internal void ToggleFlag()
    {
        if (IsRevealed)
            return;

        State = State == CellState.Flagged ? CellState.Hidden : CellState.Flagged;
    }

    internal void PlaceMine()
    {
        if (State is not CellState.Hidden)
            throw new InvalidOperationException("Cannot place a mine on a revealed or flagged cell.");

        IsMine = true;
        NeighborMinesCount = -1;
    }

    internal void RevealMine()
    {
        if (IsRevealed || !IsMine)
            return;

        State = CellState.MineRevealed;
    }

    internal void IncreaseNeighborMinesCount()
    {
        if (State is not CellState.Hidden)
            throw new InvalidOperationException("Cannot set neighbor mines count on a revealed or flagged cell.");

        NeighborMinesCount++;
    }
}