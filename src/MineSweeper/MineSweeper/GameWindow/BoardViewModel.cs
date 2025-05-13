using CommunityToolkit.Mvvm.ComponentModel;

using MineSweeper.Domain;

using System.Collections.ObjectModel;

namespace MineSweeper.GameWindow;

public sealed class BoardViewModel : ObservableObject
{
    public Board Board { get; }
    public ObservableCollection<CellViewModel> Cells { get; }

    private readonly Dictionary<Position, CellViewModel> _cellsByPosition;

    public int RowsCount => Board.RowsCount;

    public int ColumnsCount => Board.ColumnsCount;

    public BoardViewModel(Board board)
    {
        Board = board;
        Cells = [.. Board.GetAllCells()
            .Select(cell => new CellViewModel(cell))];
        _cellsByPosition = Cells.ToDictionary(cell => cell.Position);
    }

    public CellViewModel GetCell(in Position position) => _cellsByPosition[position];
}
