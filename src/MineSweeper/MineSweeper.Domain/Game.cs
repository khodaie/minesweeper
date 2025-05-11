namespace MineSweeper.Domain;

public sealed class Game
{
    public required Board Board { get; init; }
    private Game()
    {        
    }

    public static Game Create(int rows, int columns, int mines)
    {
        var board = Board.CreateInstance(rows, columns, mines);
        board.PlaceMines();
        board.Start();
        
        var game = new Game()
        {
            Board = board
        };

        return game;
    }

    public void RevealCell(int row, int column)
    {
        var cell = Board.GetCell(row, column);
        cell.Reveal();
    }
    public void FlagCell(int row, int column)
    {
        var cell = Board.GetCell(row, column);
        cell.ToggleFlag();
    }
}
