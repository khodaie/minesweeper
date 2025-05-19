using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace MineSweeper.Domain;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public sealed class Board : IBoard
{
    public int RowsCount { get; }

    public int ColumnsCount { get; }

    private ICell[,] Cells { get; }

    public ICell this[int row, int column] => GetCell(new Position(row, column));

    public ICell this[in Position position] => GetCell(position);

    private Board(int rows, int columns)
    {
        RowsCount = rows;
        ColumnsCount = columns;
        Cells = new ICell[rows, columns];
    }

    internal static Board CreateInstance(int rows, int columns)
    {
        var board = new Board(rows, columns);

        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
            board.Cells[row, column] = Cell.CreateInstance(row, column);

        return board;
    }

    public void PlaceMine(in Position position)
    {
        ValidatePosition(position);

        Cells[position.Row, position.Column].PlaceMine();

        foreach (var cell in GetNeighborCells(position))
        {
            if (cell.IsMine) continue;

            cell.IncreaseNeighborMinesCount();
        }
    }

    [Pure]
    public ICell GetCell(in Position position)
    {
        ValidatePosition(position);

        return Cells[position.Row, position.Column];
    }

    [Pure]
    public IEnumerable<ICell> GetNeighborCells(Position position)
    {
        for (var row = position.Row - 1; row <= position.Row + 1; row++)
        for (var column = position.Column - 1; column <= position.Column + 1; column++)
        {
            if (row == position.Row && column == position.Column) continue;

            if (row < 0 || row >= RowsCount || column < 0 || column >= ColumnsCount) continue;

            yield return Cells[row, column];
        }
    }

    [Pure]
    public int GetAdjacentMinesCount(in Position position)
    {
        return GetNeighborCells(position).Count(adjacentCell => adjacentCell.IsMine);
    }

    [Pure]
    public int GetUnrevealedCellsCount()
    {
        var count = 0;

        for (var row = 0; row < RowsCount; row++)
        for (var column = 0; column < ColumnsCount; column++)
            if (Cells[row, column] is { IsRevealed: false })
                count++;

        return count;
    }

    public void RevealCell(in Position position, out IReadOnlyCollection<ICell> affectedCells)
    {
        ValidatePosition(position);

        var cell = Cells[position.Row, position.Column];

        RevealCell(cell, out affectedCells);
    }

    public void RevealCell(ICell cell, out IReadOnlyCollection<ICell> affectedCells)
    {
        if (cell.IsRevealed || cell.IsFlagged)
        {
            affectedCells = [];
            return;
        }

        var handledCells = new HashSet<ICell>();
        RevealRecursive(cell, handledCells);
        affectedCells = handledCells;
    }

    [Pure]
    public bool AreAllCellsRevealedOrFlagged()
    {
        for (var row = 0; row < RowsCount; row++)
        for (var column = 0; column < ColumnsCount; column++)
            switch (Cells[row, column])
            {
                case { IsFlagged: false, IsRevealed: false }:
                case { IsFlagged: true, IsMine: false }:
                    return false;
            }

        return true;
    }

    [Pure]
    public IEnumerable<ICell> GetAllCells()
    {
        for (var row = 0; row < RowsCount; row++)
        for (var column = 0; column < ColumnsCount; column++)
            yield return Cells[row, column];
    }

    public int GetUnrevealedMinesCount()
    {
        return GetAllCells().Count(c => c is { IsRevealed: false, IsFlagged: false, IsMine: true });
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

            if (Cells[row, column].IsMine) continue;

            PlaceMine(new Position(row, column));

            placedMines++;
        }
    }

    internal void PlaceMines(IEnumerable<Position> minePositions)
    {
        foreach (var minePosition in minePositions.Distinct())
        {
            if (GetCell(minePosition).IsMine) continue;

            PlaceMine(minePosition);
        }
    }

    void IBoard.PlaceMines(int minesCount, Random random)
    {
        PlaceMines(minesCount, random);
    }

    void IBoard.PlaceMines(IEnumerable<Position> minePositions)
    {
        PlaceMines(minePositions);
    }

    private void RevealRecursive(ICell cell, HashSet<ICell> handledCells)
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
            foreach (var adjacentCell in GetNeighborCells(cell.Position))
                RevealRecursive(adjacentCell, handledCells);
    }

    private void ValidatePosition(in Position position)
    {
        position.Validate(RowsCount, ColumnsCount);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    private string GetDebuggerDisplay()
    {
        var sb = new StringBuilder();
        for (var row = 0; row < RowsCount; row++)
        {
            for (var col = 0; col < ColumnsCount; col++)
            {
                var cell = Cells[row, col];
                if (cell.IsRevealed)
                {
                    if (cell.IsMine)
                        sb.Append('*');
                    else if (cell.NeighborMinesCount > 0)
                        sb.Append(cell.NeighborMinesCount);
                    else
                        sb.Append(' ');
                }
                else if (cell.IsFlagged)
                {
                    sb.Append('F');
                }
                else if (cell.IsQuestionMarked)
                {
                    sb.Append('?');
                }
                else
                {
                    sb.Append('#');
                }
            }

            if (row < RowsCount - 1)
                sb.AppendLine("|");
        }

        return sb.ToString();
    }
}