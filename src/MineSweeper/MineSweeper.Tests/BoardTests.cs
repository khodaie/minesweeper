using MineSweeper.Domain;

namespace MineSweeper.Tests;

public sealed class BoardTests
{
    [Fact]
    public void CreateInstance_ShouldInitializeBoardWithCorrectDimensions()
    {
        // Arrange
        const int rows = 5;
        const int columns = 5;
        const int mines = 3;

        // Act
        var board = Board.CreateInstance(rows, columns, mines);

        // Assert
        Assert.Equal(rows, board.Rows);
        Assert.Equal(columns, board.Columns);
        Assert.Equal(mines, board.Mines);
    }

    [Fact]
    public void PlaceMines_ShouldPlaceCorrectNumberOfMines()
    {
        // Arrange
        const int rows = 5;
        const int columns = 5;
        const int mines = 3;
        var board = Board.CreateInstance(rows, columns, mines);

        // Act
        board.PlaceMines();

        // Assert
        var mineCount = 0;
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                if (board[row, column].IsMine)
                {
                    mineCount++;
                }
            }
        }
        Assert.Equal(mines, mineCount);
    }

    [Fact]
    public void GetCell_ShouldReturnCorrectCell()
    {
        // Arrange
        const int rows = 5;
        const int columns = 5;
        var board = Board.CreateInstance(rows, columns, 0);

        // Act
        var cell = board.GetCell(2, 3);

        // Assert
        Assert.Equal(2, cell.Row);
        Assert.Equal(3, cell.Column);
    }

    [Fact]
    public void Start_ShouldChangeStateToInProgress()
    {
        // Arrange
        var board = Board.CreateInstance(5, 5, 3);

        // Act
        board.Start();

        // Assert
        Assert.Equal(BoardState.InProgress, board.State);
    }

    [Fact]
    public void GameOver_ShouldChangeStateToGameOver()
    {
        // Arrange
        var board = Board.CreateInstance(5, 5, 3);
        board.Start();

        // Act
        board.GameOver();

        // Assert
        Assert.Equal(BoardState.GameOver, board.State);
    }

    [Fact]
    public void Win_ShouldChangeStateToWon()
    {
        // Arrange
        var board = Board.CreateInstance(5, 5, 3);
        board.Start();

        // Act
        board.Win();

        // Assert
        Assert.Equal(BoardState.Won, board.State);
    }

    [Fact]
    public void GetAdjacentCells_ShouldReturnCorrectNeighbors()
    {
        // Arrange
        var board = Board.CreateInstance(3, 3, 0);
        var cell = board.GetCell(1, 1);

        // Act
        var adjacentCells = board.GetAdjacentCells(cell).ToList();

        // Assert
        Assert.Equal(8, adjacentCells.Count);
        Assert.Contains(adjacentCells, c => c.Row == 0 && c.Column == 0);
        Assert.Contains(adjacentCells, c => c.Row == 0 && c.Column == 1);
        Assert.Contains(adjacentCells, c => c.Row == 0 && c.Column == 2);
        Assert.Contains(adjacentCells, c => c.Row == 1 && c.Column == 0);
        Assert.Contains(adjacentCells, c => c.Row == 1 && c.Column == 2);
        Assert.Contains(adjacentCells, c => c.Row == 2 && c.Column == 0);
        Assert.Contains(adjacentCells, c => c.Row == 2 && c.Column == 1);
        Assert.Contains(adjacentCells, c => c.Row == 2 && c.Column == 2);
    }
}
