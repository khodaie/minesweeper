namespace MineSweeper.Domain.Solver;

public interface ICellSuggestion
{
    IReadOnlyList<SuggestionResult> SuggestCellToReveal(IUserBoard userBoard);
}