namespace MineSweeper.Domain.Solver;

internal sealed record ManualUserCell : IUserCell
{
    public Position Position { get; }
    public CellState State { get; }
    public sbyte? NeighborMinesCount { get; }
    public bool IsHidden { get; }

    internal ManualUserCell(Position position, CellState state, sbyte? neighborMinesCount, bool isHidden)
    {
        Position = position;
        State = state;
        NeighborMinesCount = neighborMinesCount;
        IsHidden = isHidden;
    }
}