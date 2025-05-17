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

    public OperationResult RevealCell(in Position position) => RevealCell(in position, out _);

    public OperationResult RevealCell(in Position position, out IReadOnlyCollection<Cell> affectedCells)
    {
        var cell = Board[position];

        Board.RevealCell(position, out affectedCells);

        if (cell.IsMine)
        {
            GameOver();
            return OperationResults.GameOver;
        }

        if (Board.AreAllCellsRevealedOrFlagged())
        {
            Win();
            return OperationResults.GameWon;
        }

        return OperationResult.Success;
    }

    public void RevealObviousNeighborCells(in Position revealedCellPosition,
        out IReadOnlyCollection<Cell> affectedCells)
    {
        var cell = Board.GetCell(revealedCellPosition);

        // Only proceed if the cell is revealed and has at least one neighbor mine
        if (cell is not { IsRevealed: true, NeighborMinesCount: > 0 })
        {
            affectedCells = [];
            return;
        }

        var neighbors = Board.GetNeighborCells(revealedCellPosition).ToList();

        var flaggedCount = neighbors.Count(n => n.IsFlagged);

        // Only reveal neighbors if the number of flagged cells equals the neighbor mine count
        if (flaggedCount != cell.NeighborMinesCount)
        {
            affectedCells = [];
            return;
        }

        var affectedCellsSet = new HashSet<Cell>();

        foreach (var neighbor in neighbors)
        {
            if (neighbor is not { IsRevealed: false, IsFlagged: false } || affectedCellsSet.Contains(neighbor))
                continue;

            Board.RevealCell(neighbor, out var currentAffectedCells);
            affectedCellsSet.UnionWith(currentAffectedCells);
        }

        affectedCells = affectedCellsSet;
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

    private void GameOver()
    {
        if (State != GameState.InProgress)
        {
            throw new InvalidOperationException("Game is not in progress.");
        }

        State = GameState.GameOver;

        foreach (var cell in Board.GetAllCells())
        {
            if (cell is { IsRevealed: false, IsMine: true })
                cell.RevealMine();
        }
    }

    private void Win()
    {
        State = GameState.Won;
    }
}