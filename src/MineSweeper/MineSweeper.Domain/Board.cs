using System.Diagnostics.Contracts;

namespace MineSweeper.Domain;

public sealed class Board
{
    public int RowsCount { get; }

    public int ColumnsCount { get; }

    private Cell[,] Cells { get; }

    public Cell this[int row, int column] => GetCell(new Position(row, column));

    public Cell this[in Position position] => GetCell(position);

    private Board(int rows, int columns)
    {
        RowsCount = rows;
        ColumnsCount = columns;
        Cells = new Cell[rows, columns];
    }

    internal static Board CreateInstance(int rows, int columns)
    {
        var board = new Board(rows, columns);

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                board.Cells[row, column] = Cell.CreateInstance(row, column);
            }
        }

        return board;
    }

    public void PlaceMine(in Position position)
    {
        ValidatePosition(position);

        Cells[position.Row, position.Column].PlaceMine();

        foreach (var cell in GetNeighborCells(position))
        {
            if (cell.IsMine)
            {
                continue;
            }

            cell.IncreaseNeighborMinesCount();
        }
    }

    [Pure]
    public Cell GetCell(in Position position)
    {
        ValidatePosition(position);

        return Cells[position.Row, position.Column];
    }

    [Pure]
    public IEnumerable<Cell> GetNeighborCells(Position position)
    {
        for (var row = position.Row - 1; row <= position.Row + 1; row++)
        {
            for (var column = position.Column - 1; column <= position.Column + 1; column++)
            {
                if (row == position.Row && column == position.Column)
                {
                    continue;
                }

                if (row < 0 || row >= RowsCount || column < 0 || column >= ColumnsCount)
                {
                    continue;
                }

                yield return Cells[row, column];
            }
        }
    }

    [Pure]
    public int GetAdjacentMinesCount(in Position position) =>
        GetNeighborCells(position).Count(adjacentCell => adjacentCell.IsMine);

    [Pure]
    public int GetUnrevealedCellsCount()
    {
        var count = 0;

        for (var row = 0; row < RowsCount; row++)
        {
            for (var column = 0; column < ColumnsCount; column++)
            {
                if (Cells[row, column] is { IsRevealed: false })
                {
                    count++;
                }
            }
        }

        return count;
    }

    public void RevealCell(in Position position, out IReadOnlyCollection<Cell> affectedCells)
    {
        ValidatePosition(position);

        var cell = Cells[position.Row, position.Column];

        RevealCell(cell, out affectedCells);
    }

    public void RevealCell(Cell cell, out IReadOnlyCollection<Cell> affectedCells)
    {
        if (cell.IsRevealed || cell.IsFlagged)
        {
            affectedCells = [];
            return;
        }

        var handledCells = new HashSet<Cell>();
        RevealRecursive(cell, handledCells);
        affectedCells = handledCells;
    }

    public bool AreAllCellsRevealedOrFlagged()
    {
        for (var row = 0; row < RowsCount; row++)
        {
            for (var column = 0; column < ColumnsCount; column++)
            {
                switch (Cells[row, column])
                {
                    case { IsRevealed: false, IsFlagged: false }:
                    case { IsFlagged: true, IsMine: false }:
                        return false;
                }
            }
        }

        return true;
    }

    [Pure]
    public IEnumerable<Cell> GetAllCells()
    {
        for (var row = 0; row < RowsCount; row++)
        {
            for (var column = 0; column < ColumnsCount; column++)
            {
                yield return Cells[row, column];
            }
        }
    }

    internal void PlaceMines(int minesCount, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);

        if (minesCount == 0)
            return;

        if (minesCount >= RowsCount * ColumnsCount)
            throw new ArgumentException("The number of the mines should be less than the number of the cells.",
                nameof(minesCount));

        var placedMines = 0;
        while (placedMines < minesCount)
        {
            var row = random.Next(RowsCount);
            var column = random.Next(ColumnsCount);

            if (Cells[row, column].IsMine)
            {
                continue;
            }

            PlaceMine(new Position(row, column));

            placedMines++;
        }
    }

    private void RevealRecursive(Cell cell, HashSet<Cell> handledCells)
    {
        if (cell.IsRevealed || cell.IsFlagged || !handledCells.Add(cell))
            return;

        cell.Reveal();

        if (cell.IsMine)
        {
            handledCells.Add(cell);
            return;
        }

        if (cell.NeighborMinesCount == 0)
        {
            foreach (var adjacentCell in GetNeighborCells(cell.Position))
            {
                RevealRecursive(adjacentCell, handledCells);
            }
        }
    }

    private void ValidatePosition(in Position position) => position.Validate(RowsCount, ColumnsCount);

    public int GetUnrevealedMinesCount() =>
        GetAllCells().Count(c => c is { IsRevealed: false, IsFlagged: false, IsMine: true });
}