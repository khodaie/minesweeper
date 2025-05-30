using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;
using MineSweeper.Domain.Solver;
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

    public GameTimer GameTimer { get; } = new(TimeProvider.System, TimeSpan.FromSeconds(0.25));

    public RelayCommand SuggestCellsCommand { get; }

    public RelayCommand ApplySuggestionsCommand { get; }

    public GameViewModel(GameInfo gameInfo, IMessenger messenger)
    {
        Game = DomainGame.Create(gameInfo.RowsCount, gameInfo.ColumnsCount, gameInfo.MinesCount);
        Board = new BoardViewModel(Game.Board, messenger);
        _messenger = messenger;

        _messenger.Register<CellRevealedMessage>(this, OnCellRevealed);
        _messenger.Register<CellToggleFlagMessage>(this, OnCellToggleFlag);
        _messenger.Register<RevealAdjacentCellsMessage>(this, OnRevealAdjacentCells);

        SuggestCellsCommand = new RelayCommand(SuggestACell, CanExecuteSuggestCells);
        ApplySuggestionsCommand = new RelayCommand(ApplySuggestions);

        GameTimer.Start();
    }

    public void Cleanup()
    {
        _messenger.UnregisterAll(this);
    }

    private bool CanExecuteSuggestCells() => Game.State == GameState.Running;

    private void SuggestACell()
    {
        ICellSuggestion solver = new CertainCellSuggestion();

        var suggestCellToReveal = solver.SuggestCellToReveal(new UserBoard(Board.Board));
        if (suggestCellToReveal is [])
            return;

        foreach (var (userCell, suggestionType, _) in suggestCellToReveal)
        {
            Board.GetCell(userCell.Position).SetSuggested(suggestionType);
        }
    }

    private void ApplySuggestions()
    {
        foreach (var cell in Board.Cells)
        {
            if (cell is { IsRevealed: false, IsSuggested: true })
            {
                switch (cell.SuggestionType)
                {
                    case SuggestionType.Reveal:
                        cell.RevealCellCommand.Execute(null);
                        break;
                    case SuggestionType.Flag:
                        cell.ToggleFlagCommand.Execute(null);
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void Refresh()
    {
        RefreshCells(Board.Cells);
        SuggestCellsCommand.NotifyCanExecuteChanged();
    }

    private static void RefreshCells(params IEnumerable<CellViewModel> cells)
    {
        foreach (var cell in cells)
            cell.Refresh();
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
        message.Cell.ClearSuggested();

        if (affectedCells.Count != 0)
            RefreshCells(affectedCells.Select(c =>
            {
                var cellViewModel = Board.GetCell(c.Position);
                cellViewModel.ClearSuggested();
                return cellViewModel;
            }));

        OnPropertyChanged(nameof(UnrevealedMinesCount));

        HandleGameResult(operationResult);
    }

    private void OnCellToggleFlag(object recipient, CellToggleFlagMessage message)
    {
        var operationResult = Game.ToggleFlag(message.Cell.Position);

        message.Cell.ClearSuggested();

        RefreshCells(message.Cell);

        OnPropertyChanged(nameof(UnrevealedMinesCount));

        HandleGameResult(operationResult);
    }

    private void OnRevealAdjacentCells(object recipient, RevealAdjacentCellsMessage message)
    {
        var operationResult = Game.RevealObviousNeighborCells(message.Cell.Cell, out var affectedCells);

        if (affectedCells.Count != 0)
            RefreshCells(affectedCells.Select(c =>
            {
                var cell = Board.GetCell(c.Position);
                cell.ClearSuggested();
                return cell;
            }));

        OnPropertyChanged(nameof(UnrevealedMinesCount));

        HandleGameResult(operationResult);
    }
}