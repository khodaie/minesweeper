namespace MineSweeper.Domain.Solver;

public readonly record struct UserCell : IUserCell
{
    private readonly ICell _cell;

    public Position Position => _cell.Position;
    public CellState State => _cell.State;
    public sbyte? NeighborMinesCount => _cell.NeighborMinesCount;

    public bool IsHidden => _cell.State is CellState.Hidden or CellState.QuestionMarked;

    private UserCell(ICell cell) => _cell = cell;

    internal static IUserCell FromCell(ICell cell) =>
        new UserCell(cell);
}