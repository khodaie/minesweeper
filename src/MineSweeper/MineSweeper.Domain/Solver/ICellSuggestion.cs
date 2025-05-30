namespace MineSweeper.Domain.Solver;

public interface ICellSuggestion
{
    IReadOnlyList<(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty)>
        SuggestCellToReveal(IUserBoard userBoard);
}