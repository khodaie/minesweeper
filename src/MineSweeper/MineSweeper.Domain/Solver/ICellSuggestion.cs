namespace MineSweeper.Domain.Solver;

public interface ICellSuggestion
{
    IReadOnlyList<UserCell> SuggestCellToReveal(UserBoard userBoard);
}