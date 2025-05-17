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

    internal void Validate(int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Row, rows);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Column, columns);
    }

    public static implicit operator Position((int row, int column) tuple) => new(tuple.row, tuple.column);
}