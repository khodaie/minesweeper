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

        while (true)
        {
            var suggestions = FindCertainSafeCells(currentBoard);
            // Only add suggestions for positions not already seen
            var newSuggestions = suggestions
                .Where(s => !seenPositions.Contains(s.Cell.Position))
                .ToList();

            if (newSuggestions.Count == 0)
                break;

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

            var newBoard = new ManualUserBoard(updatedCells, initialBoard.RowsCount, initialBoard.ColumnsCount);

            foreach (var pos in newSuggestions.Select(s => s.Cell.Position))
                seenPositions.Add(pos);
            foreach (var pos in revealedPositions)
                seenPositions.Add(pos);

            currentBoard = newBoard;
        }

        return allSuggestions;
    }

    private static (IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)[]
        FindCertainSafeCells(IUserBoard board)
    {
        var suggestions = new List<(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)>();

        foreach (var cell in board.GetAllCells()
                     .Where(cell => cell is { State: CellState.Revealed, NeighborMinesCount: > 0 }))
        {
            var neighborCells = board.GetNeighborCells(cell.Position)
                .ToList();

            var hiddenNeighbors = neighborCells.Where(nc => nc.IsHidden).ToList();
            var flaggedNeighbors = neighborCells.Count(nc => nc.State == CellState.Flagged);

            if (hiddenNeighbors is not [] &&
                hiddenNeighbors.Count == cell.NeighborMinesCount!.Value - flaggedNeighbors)
            {
                suggestions.AddRange(hiddenNeighbors.Select(hidden =>
                    (hidden, SuggestionType.Flag, SuggestionCertainty.Certain)));
            }

            if (flaggedNeighbors == cell.NeighborMinesCount!.Value && hiddenNeighbors is not [])
            {
                suggestions.AddRange(hiddenNeighbors.Select(hidden =>
                    (hidden, SuggestionType.Reveal, SuggestionCertainty.Certain)));
            }
        }

        return suggestions
            .GroupBy(s => s.Cell.Position)
            .Select(g => g.First())
            .ToArray();
    }
}