namespace MineSweeper.Domain.Solver;

public interface IUserBoard
{
    int RowsCount { get; }
    int ColumnsCount { get; }

    int MinesCount { get; }

    int RemainingMinesCount { get; }
    IEnumerable<IUserCell> GetAllCells();
    IUserCell GetCell(in Position pos);
    IEnumerable<IUserCell> GetNeighborCells(in Position position);
}