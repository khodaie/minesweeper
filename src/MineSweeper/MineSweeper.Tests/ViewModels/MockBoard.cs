using MineSweeper.Domain;

namespace MineSweeper.Tests.ViewModels;

public sealed class MockBoard : IBoard
{
    public int RowsCount { get; set; }
    public int ColumnsCount { get; set; }

    private readonly Dictionary<Position, ICell> _cells = [];

    public ICell this[int row, int column] => _cells[(row, column)];
    public ICell this[in Position position] => _cells[position];

    public MockBoard()
    {
        RowsCount = 0;
        ColumnsCount = 0;
    }

    public MockBoard(int rows, int columns, IEnumerable<ICell>? cells = null)
    {
        RowsCount = rows;
        ColumnsCount = columns;
        if (cells != null)
        {
            foreach (var cell in cells)
                _cells[cell.Position] = cell;
        }
    }

    public void PlaceMine(in Position position)
    {
        if (_cells.TryGetValue(position, out var cell) && cell is MockCell mockCell)
            mockCell.IsMine = true;
    }

    public ICell GetCell(in Position position)
    {
        return _cells[position];
    }

    public IEnumerable<ICell> GetNeighborCells(Position position)
    {
        (int dr, int dc)[] deltas =
        [
            (-1, -1),
            (-1, 0),
            (-1, 1),
            (0, -1),
            (0, 1),
            (1, -1),
            (1, 0),
            (1, 1)
        ];

        foreach (var (dr, dc) in deltas)
        {
            var nr = position.Row + dr;
            var nc = position.Column + dc;
            var neighborPos = new Position(nr, nc);
            if (nr >= 0 && nr < RowsCount && nc >= 0 && nc < ColumnsCount &&
                _cells.TryGetValue(neighborPos, out var cell))
                yield return cell;
        }
    }

    public int GetAdjacentMinesCount(in Position position) => GetNeighborCells(position).Count(c => c.IsMine);

    public int GetUnrevealedCellsCount() => _cells.Values.Count(c => !c.IsRevealed);

    public void RevealCell(in Position position, out IReadOnlyCollection<ICell> affectedCells)
    {
        var cell = _cells[position];
        if (cell is MockCell mockCell)
            mockCell.IsRevealed = true;
        affectedCells = [cell];
    }

    public void RevealCell(ICell cell, out IReadOnlyCollection<ICell> affectedCells)
    {
        if (cell is MockCell mockCell)
            mockCell.IsRevealed = true;
        affectedCells = [cell];
    }

    public bool AreAllCellsRevealedOrFlagged() => _cells.Values.All(c => c.IsRevealed || c.IsFlagged);

    public IEnumerable<ICell> GetAllCells() => _cells.Values;

    public int GetUnrevealedMinesCount() => _cells.Values.Count(c => c.IsMine && !c.IsRevealed);

    void IBoard.PlaceMines(int minesCount, Random random)
    {
        // No-op for mock
    }

    void IBoard.PlaceMines(IEnumerable<Position> minePositions)
    {
        foreach (var pos in minePositions)
            PlaceMine(pos);
    }
}