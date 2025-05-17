using CommunityToolkit.Mvvm.ComponentModel;
using MineSweeper.Domain;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;

namespace MineSweeper.GameWindow;

public sealed class BoardViewModel : ObservableObject
{
    public Board Board { get; }

    public ObservableCollection<CellViewModel> Cells { get; }

    private readonly Dictionary<Position, CellViewModel> _cellsByPosition;

    public int RowsCount => Board.RowsCount;

    public int ColumnsCount => Board.ColumnsCount;

    public BoardViewModel(Board board, IMessenger messenger)
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