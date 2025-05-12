namespace MineSweeper.Domain;

public readonly record struct Position
{
    public int Row { get; }

    public int Column { get; }

    public Position(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(column);

        Row = row;
        Column = column;
    }

    internal void Validate(int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Row, rows);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(Column, columns);
    }   
}
