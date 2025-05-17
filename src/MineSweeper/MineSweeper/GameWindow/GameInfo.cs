namespace MineSweeper.GameWindow;

public sealed record GameInfo
{
    public GameInfo()
    {
    }

    public GameInfo(int RowsCount, int ColumnsCount, int MinesCount)
    {
        this.RowsCount = RowsCount;
        this.ColumnsCount = ColumnsCount;
        this.MinesCount = MinesCount;
    }

    public int RowsCount { get; init; }
    public int ColumnsCount { get; init; }
    public int MinesCount { get; init; }

    public void Deconstruct(out int Rows, out int Columns, out int Mines)
    {
        Rows = RowsCount;
        Columns = ColumnsCount;
        Mines = MinesCount;
    }
}