namespace MineSweeper.Domain.Solver;

public record struct SuggestionResult(IUserCell Cell, SuggestionType Type, SuggestionCertainty Certainty);