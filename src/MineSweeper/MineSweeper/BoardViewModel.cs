using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;

namespace MineSweeper;

public sealed class BoardViewModel : ObservableObject
{
    public IBoard Board { get; }

    public ObservableCollection<CellViewModel> Cells { get; }

    private readonly Dictionary<Position, CellViewModel> _cellsByPosition;

    public int RowsCount => Board.RowsCount;

    public int ColumnsCount => Board.ColumnsCount;

    public BoardViewModel(IBoard board, IMessenger messenger)
    {
        Board = board;
        Cells =
        [
            .. Board.GetAllCells()
                .Select(cell => new CellViewModel(cell, messenger))
        ];
        _cellsByPosition = Cells.ToDictionary(cell => cell.Position);
    }

    public CellViewModel GetCell(in Position position) => _cellsByPosition[position];

    public CellViewModel GetCell(int row, int column) => GetCell(new Position(row, column));
}