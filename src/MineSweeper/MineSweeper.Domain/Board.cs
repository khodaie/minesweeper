using System.Diagnostics.Contracts;

namespace MineSweeper.Domain;

public sealed class Board
{
    public int Rows { get; }

    public int Columns { get; }

    public int Mines { get; }

    private Cell[,] Cells { get; }

    public BoardState State { get; private set; }

    private Board(int rows, int columns, int mines)
    {
        Rows = rows;
        Columns = columns;
        Mines = mines;
        Cells = new Cell[rows, columns];
    }

    internal static Board CreateInstance(int rows, int columns, int mines)
    {
        var board = new Board(rows, columns, mines);

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                board.Cells[row, column] = Cell.CreateInstance(board, row, column);
            }
        }

        return board;
    }

    internal void PlaceMines() => PlaceMines(Random.Shared);

    internal void PlaceMines(Random random)
    {
        ArgumentNullException.ThrowIfNull(random, nameof(random));

        if (State != BoardState.Initializing)
        {
            throw new InvalidOperationException("Mines have already been placed.");
        }

        var placedMines = 0;
        while (placedMines < Mines)
        {
            var row = random.Next(Rows);
            var column = random.Next(Columns);

            if (Cells[row, column].IsMine)
            {
                continue;
            }

            Cells[row, column].PlaceMine();
            placedMines++;
        }
    }

    public Cell GetCell(int row, int column)
    {
        if (row < 0 || row >= Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row), "Row coordinate is out of bounds.");
        }

        if (column < 0 || column >= Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(column), "Column coordinate is out of bounds.");
        }

        return Cells[row, column];
    }

    public Cell this[int row, int column] => GetCell(row, column);

    internal void Start()
    {
        if (State != BoardState.Initializing)
        {
            throw new InvalidOperationException("Game has already started.");
        }

        State = BoardState.InProgress;
    }

    internal void GameOver()
    {
        if (State != BoardState.InProgress)
        {
            throw new InvalidOperationException("Game is not in progress.");
        }

        State = BoardState.GameOver;
    }

    internal void Win()
    {
        if (State != BoardState.InProgress)
        {
            throw new InvalidOperationException("Game is not in progress.");
        }

        State = BoardState.Won;
    }

    [Pure]
    internal IEnumerable<Cell> GetAdjacentCells(Cell cell)
    {
        ArgumentNullException.ThrowIfNull(cell);

        for (var row = cell.Row - 1; row <= cell.Row + 1; row++)
        {
            for (var column = cell.Column - 1; column <= cell.Column + 1; column++)
            {
                if (row == cell.Row && column == cell.Column)
                {
                    continue;
                }
                if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                {
                    continue;
                }
                yield return Cells[row, column];
            }
        }
    }

    [Pure]
    internal int GetUnrevealedCellsCount()
    {
        var count = 0;

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                if (!Cells[row, column].IsRevealed)
                {
                    count++;
                }
            }
        }

        return count;
    }
}