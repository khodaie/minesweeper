using MineSweeper.Domain;
using MineSweeper.Domain.Solver;

namespace MineSweeper.Tests.Domain.Solver;

public sealed class CertainCellSuggestionTests
{
    [Fact]
    public void SuggestCellToReveal_RevealsSafeNeighbor_WhenAllMinesFlagged()
    {
        // Arrange: 3x1 board, middle cell revealed with 1 mine, one flagged, one hidden
        var cells = new[]
        {
            new TestUserCell(new Position(0, 0), CellState.Flagged, null, false),
            new TestUserCell(new Position(1, 0), CellState.Revealed, 1, false),
            new TestUserCell(new Position(2, 0), CellState.Hidden, null, true)
        };
        var board = new TestUserBoard(cells, 3, 1, 1, 0);
        var suggestion = new CertainCellSuggestion();

        // Act
        var results = suggestion.SuggestCellToReveal(board);

        // Assert
        Assert.Single(results);
        Assert.Equal(new Position(2, 0), results[0].Cell.Position);
        Assert.Equal(SuggestionType.Reveal, results[0].Type);
        Assert.Equal(SuggestionCertainty.Certain, results[0].Certainty);
    }

    private sealed class TestUserBoard : IUserBoard
    {
        private readonly Dictionary<Position, IUserCell> _cells;
        private readonly int _rows;
        private readonly int _cols;
        public int RowsCount => _rows;
        public int ColumnsCount => _cols;
        public int MinesCount { get; }
        public int RemainingMinesCount { get; }

        public TestUserBoard(IEnumerable<IUserCell> cells, int rows, int cols, int mines, int remainingMines)
        {
            _cells = cells.ToDictionary(c => c.Position);
            _rows = rows;
            _cols = cols;
            MinesCount = mines;
            RemainingMinesCount = remainingMines;
        }

        public IEnumerable<IUserCell> GetAllCells() => _cells.Values;

        public IUserCell GetCell(in Position pos) => _cells[pos];

        public IEnumerable<IUserCell> GetNeighborCells(Position position)
        {
            foreach (var n in position.GetNeighborPositions(_rows, _cols))
                if (_cells.TryGetValue(n, out var cell))
                    yield return cell;
        }
    }

    private sealed class TestUserCell : IUserCell
    {
        public Position Position { get; }
        public CellState State { get; }
        public sbyte? NeighborMinesCount { get; }
        public bool IsHidden { get; }

        public TestUserCell(Position pos, CellState state, sbyte? neighborMines, bool isHidden)
        {
            Position = pos;
            State = state;
            NeighborMinesCount = neighborMines;
            IsHidden = isHidden;
        }
    }
}