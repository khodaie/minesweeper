using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;

namespace MineSweeper;

public sealed class CellViewModel : ObservableObject
{
    private readonly IMessenger _messenger;
    private readonly RelayCommand _revealCellCommand, _toggleFlagCommand;

    public ICommand RevealCellCommand => _revealCellCommand;

    public ICommand ToggleFlagCommand => _toggleFlagCommand;

    public Cell Cell { get; }

    public CellState State => Cell.State;

    public Position Position => Cell.Position;

    public int NeighborMinesCount => Cell.NeighborMinesCount;

    public bool IsRevealed => Cell.IsRevealed;

    public bool IsFlagged => Cell.IsFlagged;

    public bool IsQuestionMarked => Cell.IsQuestionMarked;

    public bool IsExploded => Cell.IsExploded;

    public bool IsMine => Cell.IsMine;

    public CellViewModel(Cell cell, IMessenger messenger)
    {
        _messenger = messenger;
        Cell = cell;

        _revealCellCommand = new RelayCommand(RevealCell, CanRevealCell);
        _toggleFlagCommand = new RelayCommand(FlagCell, CanExecuteFlagCommand);
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
    }

    private bool CanRevealCell() => this is { IsRevealed: false, IsFlagged: false };

    private void RevealCell()
    {
        _messenger.Send(new CellRevealedMessage(this));

        Refresh();
    }

    private void FlagCell()
    {
        _messenger.Send(new CellToggleFlagMessage(this));

        Refresh();
    }

    private bool CanExecuteFlagCommand() => this is { IsRevealed: false };
}