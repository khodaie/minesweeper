namespace MineSweeper.Domain.Solver;

public sealed class UserBoard(IBoard board) : IUserBoard
{
    private IBoard Board { get; } = board;

    private readonly Dictionary<Position, IUserCell> _allPositions = board.GetAllCells()
        .ToDictionary(c => c.Position, UserCell.FromCell);

    public int RowsCount => Board.RowsCount;
    public int ColumnsCount => Board.ColumnsCount;

    public IEnumerable<IUserCell> GetAllCells() =>
        Board.GetAllCells().Select(UserCell.FromCell);

    public IUserCell GetCell(in Position pos) =>
        UserCell.FromCell(Board.GetCell(in pos));

    public IEnumerable<IUserCell> GetNeighborCells(in Position position)
    {
        return GetNeighborPositions(position)
            .Select(p => _allPositions[p]);
    }

    private IEnumerable<Position> GetNeighborPositions(in Position position)
    {
        return position.GetNeighborPositions(Board.RowsCount, Board.ColumnsCount)
            .Where(neighborPosition => _allPositions.ContainsKey(neighborPosition));
    }
}