namespace MineSweeper.Domain;

public sealed class Cell
{
    public CellState State { get; private set; }

    public Board Board { get; }

    public int Row { get; }

    public int Column { get; }

    public bool IsMine { get; private set; }

    public int NeighborMinesCount { get; private set; }

    public bool IsRevealed => State == CellState.Revealed;

    public bool IsFlagged => State == CellState.Flagged;

    private Cell(Board board, int row, int column)
    {
        Board = board;
        Row = row;
        Column = column;
    }

    internal static Cell CreateInstance(Board board, int row, int column)
    {
        ArgumentNullException.ThrowIfNull(board);

        return new Cell(board, row, column);
    }

    internal void PlaceMine()
    {
        if (Board.State != BoardState.Initializing)
        {
            throw new InvalidOperationException("Mines can only be placed during initialization.");
        }

        if (IsMine)
            return;

        IsMine = true;

        // Update NeighborMinesCount for adjacent cells
        foreach (var adjacentCell in Board.GetAdjacentCells(this))
        {
            adjacentCell.NeighborMinesCount++;
        }
    }

    public void Reveal()
    {
        if (IsRevealed)
            return;

        if (IsMine)
        {
            State = CellState.Revealed;
            Board.GameOver();
            return;
        }

        RevealRecursive([]);        
    }

    public void ToggleFlag()
    {
        if (IsRevealed)
            return;

        State = State == CellState.Flagged ? CellState.Hidden : CellState.Flagged;
    }

    private void RevealRecursive(HashSet<Cell> handledCells)
    {
        if (IsRevealed || handledCells.Contains(this))
            return;

        handledCells.Add(this);

        State = CellState.Revealed;

        if (NeighborMinesCount == 0)
        {
            foreach (var adjacentCell in Board.GetAdjacentCells(this))
            {
                adjacentCell.RevealRecursive(handledCells);
            }
        }
    }
}
