using MineSweeper.Domain;

namespace MineSweeper.Tests.ViewModels;

public sealed class MockGame : IGame
{
    public GameState State { get; set; } = GameState.Initializing;
    public required IBoard Board { get; init; }
    public int MinesCount { get; set; } = 0;

    public OperationResult RevealCell(in Position position)
        => OperationResult.Success;

    public OperationResult RevealCell(in Position position, out IReadOnlyCollection<ICell> affectedCells)
    {
        affectedCells = new List<ICell>();
        return OperationResult.Success;
    }

    public OperationResult RevealCell(ICell cell, out IReadOnlyCollection<ICell> affectedCells)
    {
        affectedCells = new List<ICell>();
        return OperationResult.Success;
    }

    public OperationResult RevealObviousNeighborCells(in Position revealedCellPosition,
        out IReadOnlyCollection<ICell> affectedCells)
    {
        affectedCells = new List<ICell>();
        return OperationResult.Success;
    }

    public OperationResult RevealObviousNeighborCells(ICell cell, out IReadOnlyCollection<ICell> affectedCells)
    {
        affectedCells = new List<ICell>();
        return OperationResult.Success;
    }

    public OperationResult ToggleFlag(in Position position)
        => OperationResult.Success;
}
