using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;
using System.Windows.Input;
using DomainGame = MineSweeper.Domain.Game;

namespace MineSweeper.GameWindow;

public sealed record GameInfo(int Rows, int Columns, int Mines);

public sealed class GameViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    private readonly RelayCommand<CellViewModel> _revealCellCommand, _flagCommand;

    public DomainGame Game { get; }
    public BoardViewModel Board { get; }

    public ICommand RevealCellCommand => _revealCellCommand;

    public ICommand FlagCommand => _flagCommand;

    public int MinesCount => Game.MinesCount;

    public GameState State => Game.State;

    public GameViewModel(GameInfo gameInfo, IMessenger messenger)
    {
        Game = DomainGame.Create(gameInfo.Rows, gameInfo.Columns, gameInfo.Mines);
        Board = new BoardViewModel(Game.Board);
        _messenger = messenger;
        _revealCellCommand = new RelayCommand<CellViewModel>(RevealCell, CanRevealCell);
        _flagCommand = new RelayCommand<CellViewModel>(FlagCell, CanExecuteFlagCommand);
    }

    public void Refresh() => RefreshCells(Board.Cells);

    private static bool CanRevealCell(CellViewModel? cell) => cell is { IsRevealed: false, IsFlagged: false };

    private void RevealCell(CellViewModel? cell)
    {
        if (cell is null)
            return;

        var operationResult = Game.RevealCell(cell.Position, out var affectedCells);

        if (affectedCells.Count != 0)
            RefreshCells(affectedCells.Select(c => Board.GetCell(c.Position)));

        _flagCommand.NotifyCanExecuteChanged();
        _revealCellCommand.NotifyCanExecuteChanged();

        HandleGameResult(operationResult);
    }

    private void FlagCell(CellViewModel? cell)
    {
        if (cell is null)
            return;
        var operationResult = Game.ToggleFlag(cell.Position);
        cell.Refresh();
        _flagCommand.NotifyCanExecuteChanged();
        _revealCellCommand.NotifyCanExecuteChanged();

        HandleGameResult(operationResult);
    }

    private static bool CanExecuteFlagCommand(CellViewModel? cell) => cell is { IsRevealed: false };

    private static void RefreshCells(IEnumerable<CellViewModel> cells)
    {
        foreach (var cell in cells)
        {
            cell.Refresh();
        }
    }

    private void HandleGameResult(OperationResult operationResult)
    {
        if (operationResult == OperationResults.GameOver)
        {
            Refresh();
            _messenger.Send(new GameOverMessage());
        }
        else if (operationResult == OperationResults.GameWon)
        {
            Refresh();
            _messenger.Send(new GameWonMessage());
        }
    }
}