namespace MineSweeper.Domain;

public interface IBoard
{
    int RowsCount { get; }
    int ColumnsCount { get; }
    ICell this[int row, int column] { get; }
    ICell this[in Position position] { get; }
    void PlaceMine(in Position position);
    ICell GetCell(in Position position);
    IEnumerable<ICell> GetNeighborCells(Position position);
    int GetAdjacentMinesCount(in Position position);
    int GetUnrevealedCellsCount();
    void RevealCell(in Position position, out IReadOnlyCollection<ICell> affectedCells);
    void RevealCell(ICell cell, out IReadOnlyCollection<ICell> affectedCells);
    bool AreAllCellsRevealedOrFlagged();
    IEnumerable<ICell> GetAllCells();
    int GetUnrevealedMinesCount();

    internal void PlaceMines(int minesCount, Random random);

    internal void PlaceMines(IEnumerable<Position> minePositions);
}