namespace MineSweeper.Domain.Solver;

public sealed class StatisticalCellSuggestion : ICellSuggestion
{
    public IReadOnlyList<UserCell> SuggestCellToReveal(UserBoard board)
    {
        // 1. Try to find a cell that is 100% a mine (for flagging, not revealing)
        var certainMines = FindCertainMines(board);
        if (certainMines is not [])
        {
            // In a real solver, you would flag this cell, not reveal it.
            // But as per interface, we return it as a suggestion (could be handled by caller).
            return certainMines;
        }

        // 2. Try to find a safe cell to reveal
        var safeCells = GetSafeCells(board).ToList();
        if (safeCells is [])
        {
            return []; // No safe cells to reveal
        }

        // 3. Use statistics to minimize risk
        var bestGuess = GetBestGuessStatistically(board, safeCells);

        // 4. If no info at all, just pick a random hidden cell
        return [bestGuess ?? safeCells.MaxBy(_ => Random.Shared.Next())];
    }

    /// <summary>
    /// Finds all cells that are 100% mines based on neighbor counts.
    /// </summary>
    private static List<UserCell> FindCertainMines(UserBoard board)
    {
        var allCells = board.GetAllCells().ToList();
        var cellByPosition = allCells.ToDictionary(c => c.Position);
        var certainMines = new List<UserCell>();

        foreach (var cell in allCells)
        {
            if (cell.State != CellState.Revealed || cell.NeighborMinesCount is null)
                continue;

            var neighborPositions = board.GetNeighborPositions(cell.Position).ToList();
            var hiddenNeighbors = neighborPositions
                .Select(pos => cellByPosition[pos])
                .Where(n => n.State == CellState.Hidden)
                .ToList();

            var flaggedNeighbors = neighborPositions
                .Select(pos => cellByPosition[pos])
                .Count(n => n.State == CellState.Flagged);

            var minesLeft = cell.NeighborMinesCount.Value - flaggedNeighbors;

            // If all remaining hidden neighbors must be mines
            if (minesLeft > 0 && hiddenNeighbors.Count == minesLeft)
            {
                certainMines.AddRange(hiddenNeighbors);
            }
        }

        // Remove duplicates (if a cell is certain from multiple neighbors)
        return certainMines
            .GroupBy(c => c.Position)
            .Select(g => g.First())
            .ToList();
    }

    private static UserCell? GetBestGuessStatistically(UserBoard board, List<UserCell> safeCells)
    {
        var allCells = board.GetAllCells().ToList();
        var cellByPosition = allCells.ToDictionary(c => c.Position);

        // Calculate global mine probability for fallback
        var totalHidden = allCells.Count(c => c.State == CellState.Hidden);
        var totalFlagged = allCells.Count(c => c.State == CellState.Flagged);
        var totalMines = board.GetUnrevealedMinesCount() + totalFlagged;
        var globalMineProb = totalHidden > 0 ? (double)(totalMines - totalFlagged) / totalHidden : 1.0;

        var minRisk = double.MaxValue;
        UserCell? bestGuess = null;

        foreach (var cell in safeCells)
        {
            var neighborPositions = board.GetNeighborPositions(cell.Position);
            var neighborRevealed = neighborPositions
                .Select(pos => cellByPosition.GetValueOrDefault(pos))
                .Where(n => n is { State: CellState.Revealed, NeighborMinesCount: not null })
                .ToList();

            // If no revealed neighbors, use global probability
            if (neighborRevealed.Count == 0)
            {
                if (globalMineProb < minRisk)
                {
                    minRisk = globalMineProb;
                    bestGuess = cell;
                }

                continue;
            }

            // For each revealed neighbor, estimate the probability this cell is a mine
            double cellMineProb = 0;
            var contributingNeighbors = 0;
            foreach (var neighbor in neighborRevealed)
            {
                var neighborHidden = board.GetNeighborPositions(neighbor.Position)
                    .Select(pos => cellByPosition[pos])
                    .Count(n => n.State == CellState.Hidden);

                var neighborFlagged = board.GetNeighborPositions(neighbor.Position)
                    .Select(pos => cellByPosition[pos])
                    .Count(n => n.State == CellState.Flagged);

                var minesLeft = neighbor.NeighborMinesCount!.Value - neighborFlagged;
                if (neighborHidden <= 0)
                    continue;

                // Probability this cell is a mine from this neighbor's perspective
                var prob = (double)minesLeft / neighborHidden;
                cellMineProb += prob;
                contributingNeighbors++;
            }

            if (contributingNeighbors == 0)
                continue;

            // Average probability from all contributing neighbors
            var avgProb = cellMineProb / contributingNeighbors;
            if (avgProb < minRisk)
            {
                minRisk = avgProb;
                bestGuess = cell;
            }
        }

        return bestGuess;
    }

    private static IEnumerable<UserCell> GetSafeCells(UserBoard board)
    {
        var allCells = board.GetAllCells().ToList();
        var cellByPosition = allCells.ToDictionary(c => c.Position);
        var flaggedPositions = allCells
            .Where(cell => cell.State == CellState.Flagged)
            .Select(cell => cell.Position)
            .ToHashSet();
        var revealedWithCount = allCells
            .Where(cell => cell is { State: CellState.Revealed, NeighborMinesCount: not null })
            .ToList();
        var obviousMinePositions = new HashSet<Position>();

        foreach (var cell in revealedWithCount)
        {
            var neighbors = board.GetNeighborPositions(cell.Position)
                .Select(pos => cellByPosition[pos])
                .Where(n => n.State == CellState.Hidden && !flaggedPositions.Contains(n.Position))
                .ToList();

            var flaggedNeighborCount = board.GetNeighborPositions(cell.Position)
                .Count(pos => flaggedPositions.Contains(pos));

            var minesLeft = cell.NeighborMinesCount!.Value - flaggedNeighborCount;
            if (minesLeft > 0 && neighbors.Count == minesLeft)
            {
                foreach (var n in neighbors)
                    obviousMinePositions.Add(n.Position);
            }
        }

        var safeCells = allCells
            .Where(cell => cell.State == CellState.Hidden
                           && !flaggedPositions.Contains(cell.Position)
                           && !obviousMinePositions.Contains(cell.Position));

        return safeCells;
    }
}
