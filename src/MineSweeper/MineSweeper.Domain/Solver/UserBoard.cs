namespace MineSweeper.Domain.Solver;

public sealed class UserBoard(IBoard board)
{
    private IBoard Board { get; } = board;

    private readonly HashSet<Position> _allPositions = board.GetAllCells()
        .Select(c => c.Position)
        .ToHashSet();

    public IEnumerable<UserCell> GetAllCells() =>
        Board.GetAllCells().Select(UserCell.FromCell);

    public IEnumerable<Position> GetNeighborPositions(Position pos)
    {
        // 8 directions
        int[] dRows = [-1, -1, -1, 0, 0, 1, 1, 1],
            dCols = [-1, 0, 1, -1, 1, -1, 0, 1];

        foreach (var (dr, dc) in dRows.Zip(dCols))
        {
            if (pos.Row + dr < 0 || pos.Row + dr >= Board.RowsCount)
                continue;

            if (pos.Column + dc < 0 || pos.Column + dc >= Board.ColumnsCount)
                continue;

            var neighbor = new Position(pos.Row + dr, pos.Column + dc);
            if (_allPositions.Contains(neighbor))
                yield return neighbor;
        }
    }

    public int GetUnrevealedMinesCount() => Board.GetUnrevealedMinesCount();
}