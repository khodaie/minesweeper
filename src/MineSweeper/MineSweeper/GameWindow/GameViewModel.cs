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

    public GameViewModel(GameInfo gameInfo, IMessenger messenger)
    {
        Game = DomainGame.Create(gameInfo.Rows, gameInfo.Columns, gameInfo.Mines);
        BoardViewModel = new BoardViewModel(Game.Board);
        _messenger = messenger;
        _revealCellCommand = new RelayCommand<CellViewModel>(RevealCell, CanRevealCell);
        _flagCommand = new RelayCommand<CellViewModel>(FlagCell, CanExecuteFlagCommand);
    }

    private bool CanRevealCell(CellViewModel? cell) => cell is { IsRevealed: false, IsFlagged: false };

    public DomainGame Game { get; }
    public BoardViewModel BoardViewModel { get; }

    public ICommand RevealCellCommand => _revealCellCommand;

    public ICommand FlagCommand => _flagCommand;

    private void RevealCell(CellViewModel? cell)
    {
        if (cell is null)
            return;

        var operationResult = Game.RevealCell(cell.Position, out var affectedCells);
        
        if (affectedCells.Count != 0)
            RefreshCells(affectedCells.Select(c => BoardViewModel.GetCell(c.Position)));

        if (operationResult == OperationResults.GameOver)
        {
            _messenger.Send(new GameOverMessage());            
        }
        else if(operationResult == OperationResults.GameWon)
        { 
            _messenger.Send(new GameWonMessage()); 
        }

        _flagCommand.NotifyCanExecuteChanged();
        _revealCellCommand.NotifyCanExecuteChanged();
    }

    private void FlagCell(CellViewModel? cell)
    {
        if (cell is null)
            return;
        Game.ToggleFlag(cell.Position);
        cell.Refresh();
        _flagCommand.NotifyCanExecuteChanged();
        _revealCellCommand.NotifyCanExecuteChanged();
    }

    private static bool CanExecuteFlagCommand(CellViewModel? cell) => cell is { IsRevealed: false };

    private static void RefreshCells(IEnumerable<CellViewModel> cells)
    {
        foreach (var cell in cells)
        {
            cell.Refresh();
        }
    }
}