namespace MineSweeper.Domain;

public interface IGame
{
    GameState State { get; }
    IBoard Board { get; init; }
    int MinesCount { get; }
    OperationResult RevealCell(in Position position);
    OperationResult RevealCell(in Position position, out IReadOnlyCollection<ICell> affectedCells);
    OperationResult RevealCell(ICell cell, out IReadOnlyCollection<ICell> affectedCells);

    OperationResult RevealObviousNeighborCells(in Position revealedCellPosition,
        out IReadOnlyCollection<ICell> affectedCells);

    OperationResult RevealObviousNeighborCells(ICell cell,
        out IReadOnlyCollection<ICell> affectedCells);

    OperationResult ToggleFlag(in Position position);
}