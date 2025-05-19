using MineSweeper.Domain;

namespace MineSweeper.Tests.ViewModels;

public sealed class BoardViewModelTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange
        var cells = new[]
        {
            new MockCell { Position = new Position(0, 0) },
            new MockCell { Position = new Position(0, 1) },
            new MockCell { Position = new Position(1, 0) },
            new MockCell { Position = new Position(1, 1) }
        };
        var board = new MockBoard(2, 2, cells);
        var messenger = new MockMessenger();

        // Act
        var vm = new BoardViewModel(board, messenger);

        // Assert
        Assert.Equal(board, vm.Board);
        Assert.Equal(2, vm.RowsCount);
        Assert.Equal(2, vm.ColumnsCount);
        Assert.Equal(4, vm.Cells.Count);
        foreach (var cell in cells)
        {
            Assert.Contains(vm.Cells, c => c.Position == cell.Position);
        }
    }

    [Fact]
    public void GetCell_ByPosition_ReturnsCorrectCellViewModel()
    {
        // Arrange
        var cell = new MockCell { Position = new Position(1, 1) };
        var board = new MockBoard(2, 2, [
            new MockCell { Position = new Position(0, 0) },
            new MockCell { Position = new Position(0, 1) },
            new MockCell { Position = new Position(1, 0) },
            cell
        ]);
        var messenger = new MockMessenger();
        var vm = new BoardViewModel(board, messenger);

        // Act
        var result = vm.GetCell(new Position(1, 1));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new Position(1, 1), result.Position);
    }

    [Fact]
    public void GetCell_ByRowAndColumn_ReturnsCorrectCellViewModel()
    {
        // Arrange
        var cell = new MockCell { Position = new Position(0, 1) };
        var board = new MockBoard(2, 2, [
            new MockCell { Position = new Position(0, 0) },
            cell,
            new MockCell { Position = new Position(1, 0) },
            new MockCell { Position = new Position(1, 1) }
        ]);
        var messenger = new MockMessenger();
        var vm = new BoardViewModel(board, messenger);

        // Act
        var result = vm.GetCell(0, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new Position(0, 1), result.Position);
    }

    [Fact]
    public void Cells_CollectionContainsAllCellViewModels()
    {
        // Arrange
        var cells = new[]
        {
            new MockCell { Position = new Position(0, 0) },
            new MockCell { Position = new Position(0, 1) },
            new MockCell { Position = new Position(1, 0) },
            new MockCell { Position = new Position(1, 1) }
        };
        var board = new MockBoard(2, 2, cells);
        var messenger = new MockMessenger();

        // Act
        var vm = new BoardViewModel(board, messenger);

        // Assert
        var positions = cells.Select(c => c.Position).ToHashSet();
        foreach (var cellVm in vm.Cells)
        {
            Assert.Contains(cellVm.Position, positions);
        }
    }
}
