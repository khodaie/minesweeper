namespace MineSweeper.Domain;

public sealed class Game
{
    public GameState State { get; private set; }

    public required Board Board { get; init; }

    public int MinesCount { get; }

    private Game(int minesCount)
    {
        MinesCount = minesCount;
    }

    public static Game Create(int rows, int columns, int mines) =>
        Create(rows, columns, mines, Random.Shared);

    public static Game Create(int rows, int columns, int mines, Random random)
    {
        var board = Board.CreateInstance(rows, columns);

        var game = new Game(mines)
        {
            Board = board
        };

        board.PlaceMines(mines, random);
        game.Start();

        return game;
    }

    public static Game Create(int rows, int columns, IEnumerable<Position> minePositions)
    {
        var board = Board.CreateInstance(rows, columns);

        var distinctMinePositions = minePositions.ToHashSet();

        var game = new Game(distinctMinePositions.Count)
        {
            Board = board
        };

        board.PlaceMines(distinctMinePositions);
        game.Start();

        return game;
    }

    public OperationResult RevealCell(in Position position) => RevealCell(in position, out _);

    public OperationResult RevealCell(in Position position, out IReadOnlyCollection<ICell> affectedCells)
    {
        var cell = Board[position];

        return RevealCell(cell, out affectedCells);
    }

    public OperationResult RevealCell(ICell cell, out IReadOnlyCollection<ICell> affectedCells)
    {
        Board.RevealCell(cell, out affectedCells);

        if (cell.IsMine)
        {
            return GameOver(out affectedCells);
        }

        if (Board.AreAllCellsRevealedOrFlagged())
        {
            return Win();
        }

        return OperationResult.Success;
    }

    public OperationResult RevealObviousNeighborCells(in Position revealedCellPosition,
        out IReadOnlyCollection<ICell> affectedCells)
    {
        var cell = Board.GetCell(revealedCellPosition);

        return RevealObviousNeighborCells(cell, out affectedCells);
    }

    public OperationResult RevealObviousNeighborCells(ICell cell,
        out IReadOnlyCollection<ICell> affectedCells)
    {
        // Only proceed if the cell is revealed and has at least one neighbor mine
        if (cell is not { IsRevealed: true, NeighborMinesCount: > 0 })
        {
            affectedCells = [];
            return OperationResult.Success;
        }

        var neighbors = Board.GetNeighborCells(cell.Position).ToList();

        var flaggedCount = neighbors.Count(n => n.IsFlagged);

        // Only reveal neighbors if the number of flagged cells equals the neighbor mine count
        if (flaggedCount != cell.NeighborMinesCount)
        {
            affectedCells = [];
            return OperationResult.Success;
        }

        var affectedCellsSet = new HashSet<ICell>();

        foreach (var neighbor in neighbors)
        {
            if (neighbor is not { IsRevealed: false, IsFlagged: false } || affectedCellsSet.Contains(neighbor))
                continue;

            Board.RevealCell(neighbor, out var currentAffectedCells);
            affectedCellsSet.UnionWith(currentAffectedCells);
        }

        if (affectedCellsSet.Any(c => c is { IsExploded: true }))
        {
            var gameOverResult = GameOver(out var gameOverAffectedCells);
            affectedCellsSet.UnionWith(gameOverAffectedCells);
            affectedCells = affectedCellsSet;
            return gameOverResult;
        }

        affectedCells = affectedCellsSet;

        if (Board.AreAllCellsRevealedOrFlagged())
        {
            return Win();
        }

        return OperationResult.Success;
    }

    public OperationResult ToggleFlag(in Position position)
    {
        var cell = Board[position];
        cell.ToggleFlag();

        if (Board.AreAllCellsRevealedOrFlagged())
        {
            Win();
            return OperationResults.GameWon;
        }

        return OperationResult.Success;
    }

    private void Start()
    {
        if (State != GameState.Initializing)
        {
            throw new InvalidOperationException("Game has already started.");
        }

        State = GameState.InProgress;
    }

    private OperationResult GameOver(out IReadOnlyCollection<ICell> affectedCells)
    {
        State = GameState.GameOver;

        var affectedCellsList = new List<ICell>();

        foreach (var cell in Board.GetAllCells())
        {
            if (cell is { IsRevealed: false, IsMine: true })
            {
                cell.RevealMine();
                affectedCellsList.Add(cell);
            }
        }

        affectedCells = affectedCellsList.AsReadOnly();

        return OperationResults.GameOver;
    }

    private OperationResult Win()
    {
        State = GameState.Won;

        return OperationResults.GameWon;
    }
}