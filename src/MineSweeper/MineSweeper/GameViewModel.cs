using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;
using DomainGame = MineSweeper.Domain.Game;

namespace MineSweeper;

public sealed class GameViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public IGame Game { get; }
    public BoardViewModel Board { get; }

    public int MinesCount => Game.MinesCount;

    public int UnrevealedMinesCount => Game.Board.GetUnrevealedMinesCount();

    public GameState State => Game.State;

    public GameTimer GameTimer { get; } = new(TimeSpan.FromSeconds(0.15));

    public GameViewModel(GameInfo gameInfo, IMessenger messenger)
    {
        Game = DomainGame.Create(gameInfo.RowsCount, gameInfo.ColumnsCount, gameInfo.MinesCount);
        Board = new BoardViewModel(Game.Board, messenger);
        _messenger = messenger;

        _messenger.Register<CellRevealedMessage>(this, OnCellRevealed);
        _messenger.Register<CellToggleFlagMessage>(this, OnCellToggleFlag);
        _messenger.Register<RevealAdjacentCellsMessage>(this, OnRevealAdjacentCells);

        GameTimer.Start();
    }

    private void Refresh()
    {
        RefreshCells(Board.Cells);
    }

    private static void RefreshCells(params IEnumerable<CellViewModel> cells)
    {
        foreach (var cell in cells) cell.Refresh();
    }

    private void HandleGameResult(OperationResult operationResult)
    {
        if (operationResult == OperationResults.GameOver)
        {
            GameTimer.Stop();
            Refresh();
            _messenger.Send(new GameOverMessage());
        }
        else if (operationResult == OperationResults.GameWon)
        {
            GameTimer.Stop();
            Refresh();
            _messenger.Send(new GameWonMessage());
        }
    }

    private void OnCellRevealed(object recipient, CellRevealedMessage message)
    {
        var operationResult = Game.RevealCell(message.Cell.Cell, out var affectedCells);

        if (affectedCells.Count != 0)
            RefreshCells(affectedCells.Select(c => Board.GetCell(c.Position)));

        OnPropertyChanged(nameof(UnrevealedMinesCount));

        HandleGameResult(operationResult);
    }

    private void OnCellToggleFlag(object recipient, CellToggleFlagMessage message)
    {
        var operationResult = Game.ToggleFlag(message.Cell.Position);

        RefreshCells(message.Cell);

        OnPropertyChanged(nameof(UnrevealedMinesCount));

        HandleGameResult(operationResult);
    }

    private void OnRevealAdjacentCells(object recipient, RevealAdjacentCellsMessage message)
    {
        var operationResult = Game.RevealObviousNeighborCells(message.Cell.Cell, out var affectedCells);

        if (affectedCells.Count != 0)
            RefreshCells(affectedCells.Select(c => Board.GetCell(c.Position)));

        OnPropertyChanged(nameof(UnrevealedMinesCount));

        HandleGameResult(operationResult);
    }
}