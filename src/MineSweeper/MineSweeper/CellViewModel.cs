using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;
using MineSweeper.Domain.Solver;

namespace MineSweeper;

public sealed class CellViewModel : ObservableObject
{
    private readonly IMessenger _messenger;
    private readonly RelayCommand _revealCellCommand, _toggleFlagCommand, _revealAdjacentCellsCommand;

    public ICommand RevealCellCommand => _revealCellCommand;

    public ICommand ToggleFlagCommand => _toggleFlagCommand;

    public ICommand RevealAdjacentCellsCommand => _revealAdjacentCellsCommand;

    public ICell Cell { get; }

    public CellState State => Cell.State;

    public Position Position => Cell.Position;

    public int NeighborMinesCount => Cell.NeighborMinesCount;

    public bool IsRevealed => Cell.IsRevealed;

    public bool IsFlagged => Cell.IsFlagged;

    public bool IsQuestionMarked => Cell.IsQuestionMarked;

    public bool IsExploded => Cell.IsExploded;

    public bool IsMine => Cell.IsMine;

    private bool _isSuggested;

    public bool IsSuggested
    {
        get => _isSuggested;
        private set
        {
            if (_isSuggested == value)
                return;

            OnPropertyChanging();
            _isSuggested = value;
            OnPropertyChanged();
        }
    }

    private SuggestionType? _suggestionType;

    public SuggestionType? SuggestionType
    {
        get => _suggestionType;

        private set
        {
            if (_suggestionType == value)
                return;

            OnPropertyChanging();
            _suggestionType = value;
            OnPropertyChanged();
        }
    }

    public CellViewModel(ICell cell, IMessenger messenger)
    {
        _messenger = messenger;
        Cell = cell;

        _revealCellCommand = new RelayCommand(RevealCell, CanRevealCell);
        _toggleFlagCommand = new RelayCommand(FlagCell, CanExecuteFlagCommand);
        _revealAdjacentCellsCommand = new RelayCommand(RevealAdjacentCells, CanExecuteRevealAdjacentCellsCommand);
    }

    public void SetSuggested(SuggestionType suggestionType)
    {
        IsSuggested = true;
        SuggestionType = suggestionType;
    }

    public void ClearSuggested()
    {
        IsSuggested = false;
        SuggestionType = null;
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(State));
        OnPropertyChanged(nameof(IsRevealed));
        OnPropertyChanged(nameof(IsFlagged));
        OnPropertyChanged(nameof(Cell));
        OnPropertyChanged(nameof(IsExploded));
        OnPropertyChanged(nameof(IsQuestionMarked));

        _revealCellCommand.NotifyCanExecuteChanged();
        _toggleFlagCommand.NotifyCanExecuteChanged();
        _revealAdjacentCellsCommand.NotifyCanExecuteChanged();
    }

    private void RevealCell()
    {
        IsSuggested = false;

        _messenger.Send(new CellRevealedMessage(this));

        Refresh();
    }

    private void FlagCell()
    {
        _messenger.Send(new CellToggleFlagMessage(this));

        Refresh();
    }

    private void RevealAdjacentCells()
    {
        _messenger.Send(new RevealAdjacentCellsMessage(this));

        Refresh();
    }

    private bool CanRevealCell() => this is { IsRevealed: false, IsFlagged: false };

    private bool CanExecuteFlagCommand() => this is { IsRevealed: false };

    private bool CanExecuteRevealAdjacentCellsCommand() =>
        this is { IsRevealed: true, IsFlagged: false, NeighborMinesCount: > 0 };
}