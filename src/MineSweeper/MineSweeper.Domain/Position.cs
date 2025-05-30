namespace MineSweeper.Domain;

public readonly record struct Position
{
    public int Row { get; }

    public int Column { get; }

    public Position(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfNegative(column);

        Row = row;
        Column = column;
    }

    public static implicit operator Position((int row, int column) tuple) => new(tuple.row, tuple.column);

    public IEnumerable<Position> GetNeighborPositions(int rowsCount, int columnsCount)
    {
        // 8 directions
        int[] dRows = [-1, -1, -1, 0, 0, 1, 1, 1],
            dCols = [-1, 0, 1, -1, 1, -1, 0, 1];

        foreach (var (dr, dc) in dRows.Zip(dCols))
        {
            if (Row + dr < 0 || Row + dr >= rowsCount)
                continue;

            if (Column + dc < 0 || Column + dc >= columnsCount)
                continue;

            var neighbor = new Position(Row + dr, Column + dc);

            yield return neighbor;
        }
    }

    internal void Validate(int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Row, rows);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Column, columns);
    }
}