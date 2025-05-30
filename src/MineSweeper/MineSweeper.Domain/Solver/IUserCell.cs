namespace MineSweeper.Domain.Solver;

public interface IUserCell
{
    Position Position { get; }
    CellState State { get; }
    sbyte? NeighborMinesCount { get; }
    bool IsHidden { get; }
}