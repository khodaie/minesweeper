namespace MineSweeper.Domain.Solver;

public readonly record struct UserCell(Position Position, CellState State, sbyte? NeighborMinesCount)
{
    internal static UserCell FromCell(ICell cell) =>
        new(cell.Position, cell.State, cell.IsRevealed ? cell.NeighborMinesCount : null);
}