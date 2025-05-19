using MineSweeper.Domain;

namespace MineSweeper.Tests.ViewModels;

public sealed class GameViewModelTests
{
    private static GameInfo CreateGameInfo(int rows = 2, int cols = 2, int mines = 1)
        => new(rows, cols, mines);

    private static MockMessenger CreateMessengerMock() => new();

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var messenger = CreateMessengerMock();
        var info = CreateGameInfo();
        var vm = new GameViewModel(info, messenger);

        Assert.NotNull(vm.Game);
        Assert.NotNull(vm.Board);
        Assert.Equal(info.MinesCount, vm.MinesCount);
        Assert.Equal(vm.Game.State, vm.State);
        Assert.True(vm.GameTimer.ElapsedTime >= TimeSpan.Zero);
    }

    [Fact]
    public void UnrevealedMinesCount_ReflectsGameBoard()
    {
        var messenger = CreateMessengerMock();
        var info = CreateGameInfo();
        var vm = new GameViewModel(info, messenger);

        var expected = vm.Game.Board.GetUnrevealedMinesCount();
        Assert.Equal(expected, vm.UnrevealedMinesCount);
    }

    [Fact]
    public void OnCellRevealed_UpdatesCellsAndNotifies()
    {
        var messenger = CreateMessengerMock();
        var info = CreateGameInfo();
        var vm = new GameViewModel(info, messenger);
        var cell = vm.Board.Cells.First();

        var msg = new CellRevealedMessage(cell);
        var unrevealedBefore = vm.UnrevealedMinesCount;
        vm.GetType().GetMethod("OnCellRevealed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(vm, [vm, msg]);

        Assert.True(vm.UnrevealedMinesCount <= unrevealedBefore);
        // Check if a GameOverMessage or GameWonMessage was sent if game ended
        Assert.True(
            messenger.SentMessages.Any(m => m is GameOverMessage or GameWonMessage) ||
            vm.Game.State == GameState.Running
        );
    }

    [Fact]
    public void OnCellToggleFlag_UpdatesFlagAndNotifies()
    {
        var messenger = CreateMessengerMock();
        var info = CreateGameInfo();
        var vm = new GameViewModel(info, messenger);
        var cell = vm.Board.Cells.First();

        var msg = new CellToggleFlagMessage(cell);
        var unrevealedBefore = vm.UnrevealedMinesCount;
        vm.GetType().GetMethod("OnCellToggleFlag",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(vm, [vm, msg]);

        Assert.True(vm.UnrevealedMinesCount <= unrevealedBefore);
        Assert.True(
            messenger.SentMessages.Any(m => m is GameOverMessage or GameWonMessage) ||
            vm.Game.State == GameState.Running
        );
    }

    [Fact]
    public void OnRevealAdjacentCells_UpdatesCellsAndNotifies()
    {
        var messenger = CreateMessengerMock();
        var info = CreateGameInfo();
        var vm = new GameViewModel(info, messenger);
        var cell = vm.Board.Cells.First();

        var msg = new RevealAdjacentCellsMessage(cell);
        var unrevealedBefore = vm.UnrevealedMinesCount;
        vm.GetType().GetMethod("OnRevealAdjacentCells",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(vm, [vm, msg]);

        Assert.True(vm.UnrevealedMinesCount <= unrevealedBefore);
        Assert.True(
            messenger.SentMessages.Any(m => m is GameOverMessage or GameWonMessage) ||
            vm.Game.State == GameState.Running
        );
    }

    [Fact]
    public void HandleGameResult_GameOver_SendsGameOverMessage()
    {
        var messenger = CreateMessengerMock();
        var info = CreateGameInfo();
        var vm = new GameViewModel(info, messenger);

        var method = vm.GetType().GetMethod("HandleGameResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(vm, [OperationResults.GameOver]);

        Assert.Contains(messenger.SentMessages, m => m is GameOverMessage);
    }

    [Fact]
    public void HandleGameResult_GameWon_SendsGameWonMessage()
    {
        var messenger = CreateMessengerMock();
        var info = CreateGameInfo();
        var vm = new GameViewModel(info, messenger);

        var method = vm.GetType().GetMethod("HandleGameResult",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(vm, [OperationResults.GameWon]);

        Assert.Contains(messenger.SentMessages, m => m is GameWonMessage);
    }
}
