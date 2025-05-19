using System.Diagnostics;

namespace MineSweeper.Domain;

[DebuggerDisplay("Cell[{Position.Row}, {Position.Column}]: {State}")]
public sealed class Cell : ICell
{
    public CellState State { get; private set; } = CellState.Hidden;

    public Position Position { get; }

    public bool IsMine { get; private set; }

    public sbyte NeighborMinesCount { get; private set; }

    public bool IsRevealed => State is CellState.Revealed or CellState.IsExploded or CellState.MineRevealed;

    public bool IsFlagged => State == CellState.Flagged;

    public bool IsQuestionMarked => State == CellState.QuestionMarked;

    public bool IsExploded => State == CellState.IsExploded;

    private Cell(in Position position)
    {
        Position = position;
    }

    public static Cell CreateInstance(int row, int column)
    {
        return CreateInstance(new Position(row, column));
    }

    public static Cell CreateInstance(in Position position)
    {
        return new Cell(position);
    }

    internal void Reveal()
    {
        if (IsRevealed || IsFlagged)
            return;

        State = IsMine ? CellState.IsExploded : CellState.Revealed;
    }

    void ICell.Reveal() => Reveal();

    internal void ToggleFlag()
    {
        if (IsRevealed)
            return;

        State = State switch
        {
            CellState.Flagged => CellState.QuestionMarked,
            CellState.QuestionMarked => CellState.Hidden,
            CellState.Hidden => CellState.Flagged,
            _ => State
        };
    }

    void ICell.ToggleFlag() => ToggleFlag();

    internal void PlaceMine()
    {
        if (State is not CellState.Hidden)
            throw new InvalidOperationException("Cannot place a mine on a revealed or flagged cell.");

        IsMine = true;
        NeighborMinesCount = -1;
    }

    void ICell.PlaceMine() => PlaceMine();

    internal void RevealMine()
    {
        if (IsRevealed || !IsMine)
            return;

        State = CellState.MineRevealed;
    }

    void ICell.RevealMine() => RevealMine();

    internal void IncreaseNeighborMinesCount()
    {
        if (State is not CellState.Hidden)
            throw new InvalidOperationException("Cannot set neighbor mines count on a revealed or flagged cell.");

        if (IsMine)
            throw new InvalidOperationException("This cell is already a mine.");

        if (NeighborMinesCount >= 8)
            throw new InvalidOperationException("Cannot increase neighbor mines count beyond 8.");

        NeighborMinesCount++;
    }

    void ICell.IncreaseNeighborMinesCount() => IncreaseNeighborMinesCount();
}