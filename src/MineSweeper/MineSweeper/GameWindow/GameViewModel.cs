using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;
using DomainGame = MineSweeper.Domain.Game;

namespace MineSweeper.GameWindow;

public sealed record GameInfo(int Rows, int Columns, int Mines);

public sealed class GameViewModel : ObservableObject
{
    private readonly IMessenger _messenger;

    public DomainGame Game { get; }
    public BoardViewModel Board { get; }

    public int MinesCount => Game.MinesCount;

    public GameState State => Game.State;

    public GameViewModel(GameInfo gameInfo, IMessenger messenger)
    {
        Game = DomainGame.Create(gameInfo.Rows, gameInfo.Columns, gameInfo.Mines);
        Board = new BoardViewModel(Game.Board, messenger);
        _messenger = messenger;

        _messenger.Register<CellRevealedMessage>(this, OnCellRevealed);
        _messenger.Register<CellToggleFlagMessage>(this, OnCellToggleFlag);
    }

    private void Refresh() => RefreshCells(Board.Cells);

    private static void RefreshCells(params IEnumerable<CellViewModel> cells)
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

    private void OnCellRevealed(object recipient, CellRevealedMessage message)
    {
        var operationResult = Game.RevealCell(message.Cell.Position, out var affectedCells);

        if (affectedCells.Count != 0)
            RefreshCells(affectedCells.Select(c => Board.GetCell(c.Position)));

        HandleGameResult(operationResult);
    }

    private void OnCellToggleFlag(object recipient, CellToggleFlagMessage message)
    {
        var operationResult = Game.ToggleFlag(message.Cell.Position);

        RefreshCells(message.Cell);

        HandleGameResult(operationResult);
    }
}