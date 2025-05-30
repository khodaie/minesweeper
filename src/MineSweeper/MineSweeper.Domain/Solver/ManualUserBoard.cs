namespace MineSweeper.Domain.Solver;

internal sealed class ManualUserBoard : IUserBoard
{
    private readonly Dictionary<Position, IUserCell> _cells;

    public int RowsCount { get; }

    public int ColumnsCount { get; }

    internal ManualUserBoard(IEnumerable<IUserCell> cells, int rowsCount, int columnsCount)
    {
        _cells = cells.ToDictionary(c => c.Position);
        RowsCount = rowsCount;
        ColumnsCount = columnsCount;
    }

    public IEnumerable<IUserCell> GetAllCells() =>
        _cells.Values;

    public IUserCell GetCell(in Position pos) =>
        _cells[pos];

    public IEnumerable<IUserCell> GetNeighborCells(in Position position)
    {
        return GetNeighborPositions(position)
            .Where(_cells.ContainsKey)
            .Select(p => _cells[p]);
    }

    private IEnumerable<Position> GetNeighborPositions(in Position position)
    {
        return position.GetNeighborPositions(RowsCount, ColumnsCount);
    }
}