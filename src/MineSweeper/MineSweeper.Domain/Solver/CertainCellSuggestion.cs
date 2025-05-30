namespace MineSweeper.Domain.Solver;

public sealed class CertainCellSuggestion : ICellSuggestion
{
    public IReadOnlyList<(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)> SuggestCellToReveal(
        IUserBoard board)
    {
        return GetAllCertainSuggestionsUntilStable(board);
    }

    private static List<(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)>
        GetAllCertainSuggestionsUntilStable(IUserBoard initialBoard)
    {
        var allSuggestions = new List<(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)>();
        var currentBoard = initialBoard;
        var seenPositions = new HashSet<Position>();

        var totalMines = initialBoard.MinesCount;
        var remainingMines = initialBoard.RemainingMinesCount;

        while (true)
        {
            var suggestions = FindCertainSafeCells(currentBoard, remainingMines)
                .Concat(FindCertainBySubsetLogic(currentBoard, remainingMines))
                .ToArray();

            // Only add suggestions for positions not already seen
            var newSuggestions = suggestions
                .Where(s => !seenPositions.Contains(s.Cell.Position))
                .ToList();

            allSuggestions.AddRange(newSuggestions);

            // Collect all positions to reveal, including recursive empty reveals
            var positionsToReveal = new HashSet<Position>(
                newSuggestions
                    .Where(s => s.Type == SuggestionType.Reveal)
                    .Select(s => s.Cell.Position)
            );

            // Recursively reveal empty cells as per game logic
            var revealedPositions = new HashSet<Position>(positionsToReveal);
            var queue = new Queue<Position>(positionsToReveal);

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                var cell = currentBoard.GetCell(pos);
                if (cell is not { State: CellState.Hidden, NeighborMinesCount: 0 })
                    continue;
                foreach (var neighbor in currentBoard.GetNeighborCells(pos))
                {
                    if (neighbor.State == CellState.Hidden && revealedPositions.Add(neighbor.Position))
                    {
                        queue.Enqueue(neighbor.Position);
                    }
                }
            }

            // Update suggestions to include all recursively revealed cells
            foreach (var pos in revealedPositions)
            {
                if (allSuggestions.Any(s => s.Cell.Position == pos && s.Type == SuggestionType.Reveal))
                    continue;

                var cell = currentBoard.GetCell(pos);
                allSuggestions.Add((cell, SuggestionType.Reveal, SuggestionCertainty.Certain));
            }

            // Apply suggestions to create a new board state
            var updatedCells = currentBoard.GetAllCells()
                .Select(cell =>
                {
                    if (revealedPositions.Contains(cell.Position) && cell.State == CellState.Hidden)
                        return new ManualUserCell(cell.Position, CellState.Revealed, cell.NeighborMinesCount, false);

                    var suggestion = newSuggestions.FirstOrDefault(s => s.Cell.Position == cell.Position);
                    if (suggestion == default)
                        return cell;

                    return suggestion.Type switch
                    {
                        SuggestionType.Flag when cell.State == CellState.Hidden => new ManualUserCell(cell.Position,
                            CellState.Flagged, cell.NeighborMinesCount, true),
                        _ => cell
                    };
                })
                .ToList();

            // Update remaining mines count based on new flags
            var flagsPlaced = updatedCells.Count(c => c.State == CellState.Flagged);
            remainingMines = totalMines - flagsPlaced;

            var newBoard = new ManualUserBoard(updatedCells, initialBoard.RowsCount, initialBoard.ColumnsCount,
                totalMines, remainingMines);

            foreach (var pos in newSuggestions.Select(s => s.Cell.Position))
                seenPositions.Add(pos);
            foreach (var pos in revealedPositions)
                seenPositions.Add(pos);

            currentBoard = newBoard;

            // There might be some other suggestions below as well. Consider all new suggestions and break if no new position is suggested.
            if (newSuggestions.Count == 0 && revealedPositions.All(pos => seenPositions.Contains(pos)))
                break;
        }

        return allSuggestions;
    }

    private static (IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)[]
        FindCertainSafeCells(IUserBoard board, int remainingMines)
    {
        var suggestions = new List<(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)>();

        foreach (var cell in board.GetAllCells()
                     .Where(cell => cell is { State: CellState.Revealed, NeighborMinesCount: > 0 }))
        {
            var neighborCells = board.GetNeighborCells(cell.Position)
                .ToList();

            var hiddenNeighbors = neighborCells.Where(nc => nc.IsHidden).ToList();
            var flaggedNeighbors = neighborCells.Count(nc => nc.State == CellState.Flagged);

            if (hiddenNeighbors.Count > 0 &&
                hiddenNeighbors.Count == cell.NeighborMinesCount!.Value - flaggedNeighbors)
            {
                // Only flag if we have enough remaining mines
                if (remainingMines >= hiddenNeighbors.Count)
                {
                    suggestions.AddRange(hiddenNeighbors.Select(hidden =>
                        (hidden, SuggestionType.Flag, SuggestionCertainty.Certain)));
                }
            }

            if (flaggedNeighbors == cell.NeighborMinesCount!.Value && hiddenNeighbors.Count > 0)
            {
                suggestions.AddRange(hiddenNeighbors.Select(hidden =>
                    (hidden, SuggestionType.Reveal, SuggestionCertainty.Certain)));
            }
        }

        // Global logic: if number of hidden cells equals remaining mines, flag all hidden cells
        var allHidden = board.GetAllCells().Where(c => c.State == CellState.Hidden).ToList();
        if (allHidden.Count > 0 && allHidden.Count == remainingMines)
        {
            suggestions.AddRange(allHidden.Select(hidden =>
                (hidden, SuggestionType.Flag, SuggestionCertainty.Certain)));
        }

        return suggestions
            .GroupBy(s => s.Cell.Position)
            .Select(g => g.First())
            .ToArray();
    }

    private static (IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)[] FindCertainBySubsetLogic(
        IUserBoard board, int remainingMines)
    {
        var suggestions = new List<(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)>();

        // Precompute revealed cells and their hidden/flagged neighbors
        var revealedCells = board.GetAllCells()
            .Where(cell => cell is { State: CellState.Revealed, NeighborMinesCount: > 0 })
            .ToList();

        // Map: cell -> (hidden set, flagged count, mines left)
        var cellInfo = new List<(IUserCell Cell, HashSet<Position> Hidden, int MinesLeft)>(revealedCells.Count);
        foreach (var cell in revealedCells)
        {
            var neighbors = board.GetNeighborCells(cell.Position);
            var flagged = 0;
            var hidden = new HashSet<Position>();
            foreach (var n in neighbors)
            {
                if (n.State == CellState.Flagged) flagged++;
                else if (n.IsHidden) hidden.Add(n.Position);
            }

            var minesLeft = cell.NeighborMinesCount!.Value - flagged;
            if (minesLeft < 0) continue; // Defensive: skip inconsistent state
            cellInfo.Add((cell, hidden, minesLeft));
        }

        // For each pair, only check if A.Hidden is a subset of B.Hidden and A != B
        for (var i = 0; i < cellInfo.Count; i++)
        {
            var (_, hiddenA, minesA) = cellInfo[i];
            if (hiddenA.Count == 0) continue;
            for (var j = 0; j < cellInfo.Count; j++)
            {
                if (i == j) continue;
                var (_, hiddenB, minesB) = cellInfo[j];
                if (hiddenA.Count >= hiddenB.Count) continue; // Only proper subsets
                if (!hiddenA.IsSubsetOf(hiddenB)) continue;

                var diff = hiddenB.Count == hiddenA.Count ? [] : hiddenB.Except(hiddenA).ToArray();
                var mineDiff = minesB - minesA;
                if (mineDiff == diff.Length && mineDiff > 0)
                {
                    // Only flag if we have enough remaining mines
                    if (remainingMines >= diff.Length)
                    {
                        suggestions.AddRange(from pos in diff
                            select board.GetCell(pos)
                            into cell
                            where cell.State == CellState.Hidden
                            select (cell, SuggestionType.Flag, SuggestionCertainty.Certain));
                    }
                }
                else if (mineDiff == 0 && diff.Length > 0)
                {
                    // All cells in diff must be safe
                    suggestions.AddRange(from pos in diff
                        select board.GetCell(pos)
                        into cell
                        where cell.State == CellState.Hidden
                        select (cell, SuggestionType.Reveal, SuggestionCertainty.Certain));
                }
            }
        }

        // Global logic: if number of hidden cells equals remaining mines, flag all hidden cells
        var allHidden = board.GetAllCells().Where(c => c.State == CellState.Hidden).ToList();
        if (allHidden.Count > 0 && allHidden.Count == remainingMines)
        {
            suggestions.AddRange(allHidden.Select(hidden =>
                (hidden, SuggestionType.Flag, SuggestionCertainty.Certain)));
        }

        return suggestions
            .GroupBy(s => s.Cell.Position)
            .Select(g => g.First())
            .ToArray();
    }
}