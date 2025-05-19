using MineSweeper.Domain;

namespace MineSweeper.Tests.ViewModels;

public sealed class CellViewModelTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var cell = new MockCell { Position = new Position(1, 2), NeighborMinesCount = 3, IsMine = true };
        var messenger = new MockMessenger();

        var vm = new CellViewModel(cell, messenger);

        Assert.Equal(cell, vm.Cell);
        Assert.Equal(cell.State, vm.State);
        Assert.Equal(cell.Position, vm.Position);
        Assert.Equal(cell.NeighborMinesCount, vm.NeighborMinesCount);
        Assert.Equal(cell.IsMine, vm.IsMine);
        Assert.Equal(cell.IsRevealed, vm.IsRevealed);
        Assert.Equal(cell.IsFlagged, vm.IsFlagged);
        Assert.Equal(cell.IsQuestionMarked, vm.IsQuestionMarked);
        Assert.Equal(cell.IsExploded, vm.IsExploded);
    }

    [Fact]
    public void RevealCellCommand_SendsMessage_AndRefreshes()
    {
        var cell = new MockCell();
        var messenger = new MockMessenger();
        var vm = new CellViewModel(cell, messenger);

        var refreshed = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.State))
                refreshed = true;
        };

        vm.RevealCellCommand.Execute(null);

        // Assert that a CellRevealedMessage was sent for this vm
        Assert.Contains(messenger.SentMessages, m =>
            m is CellRevealedMessage msg && msg.Cell == vm);

        Assert.True(refreshed);
    }

    [Fact]
    public void ToggleFlagCommand_SendsMessage_AndRefreshes()
    {
        var cell = new MockCell();
        var messenger = new MockMessenger();
        var vm = new CellViewModel(cell, messenger);

        var refreshed = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.State)) refreshed = true;
        };

        vm.ToggleFlagCommand.Execute(null);

        // Assert that a CellToggleFlagMessage was sent for this vm
        Assert.Contains(messenger.SentMessages, m =>
            m is CellToggleFlagMessage msg && msg.Cell == vm);

        Assert.True(refreshed);
    }

    [Fact]
    public void RevealAdjacentCellsCommand_SendsMessage_AndRefreshes()
    {
        var cell = new MockCell { IsRevealed = true, NeighborMinesCount = 2 };
        var messenger = new MockMessenger();
        var vm = new CellViewModel(cell, messenger);

        var refreshed = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.State)) refreshed = true;
        };

        vm.RevealAdjacentCellsCommand.Execute(null);

        // Assert that a RevealAdjacentCellsMessage was sent for this vm
        Assert.Contains(messenger.SentMessages, m =>
            m is RevealAdjacentCellsMessage msg && msg.Cell == vm);

        Assert.True(refreshed);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public void CanRevealCell_RespectsState(bool isRevealed, bool isFlagged, bool expected)
    {
        var cell = new MockCell { IsRevealed = isRevealed, IsFlagged = isFlagged };
        var messenger = new MockMessenger();
        var vm = new CellViewModel(cell, messenger);

        var canExecute = vm.RevealCellCommand.CanExecute(null);

        Assert.Equal(expected, canExecute);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void CanExecuteFlagCommand_RespectsState(bool isRevealed, bool expected)
    {
        var cell = new MockCell { IsRevealed = isRevealed };
        var messenger = new MockMessenger();
        var vm = new CellViewModel(cell, messenger);

        var canExecute = vm.ToggleFlagCommand.CanExecute(null);

        Assert.Equal(expected, canExecute);
    }

    [Theory]
    [InlineData(true, false, 1, true)]
    [InlineData(true, true, 1, false)]
    [InlineData(false, false, 1, false)]
    [InlineData(true, false, 0, false)]
    public void CanExecuteRevealAdjacentCellsCommand_RespectsState(bool isRevealed, bool isFlagged, int neighborMines,
        bool expected)
    {
        var cell = new MockCell
            { IsRevealed = isRevealed, IsFlagged = isFlagged, NeighborMinesCount = (sbyte)neighborMines };
        var messenger = new MockMessenger();
        var vm = new CellViewModel(cell, messenger);

        var canExecute = vm.RevealAdjacentCellsCommand.CanExecute(null);

        Assert.Equal(expected, canExecute);
    }

    [Fact]
    public void Refresh_RaisesPropertyChanged_AndNotifiesCommands()
    {
        var cell = new MockCell();
        var messenger = new MockMessenger();
        var vm = new CellViewModel(cell, messenger);

        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Refresh();

        Assert.Contains(nameof(vm.State), changed);
        Assert.Contains(nameof(vm.IsRevealed), changed);
        Assert.Contains(nameof(vm.IsFlagged), changed);
        Assert.Contains(nameof(vm.Cell), changed);
        Assert.Contains(nameof(vm.IsExploded), changed);
        Assert.Contains(nameof(vm.IsQuestionMarked), changed);
    }
}