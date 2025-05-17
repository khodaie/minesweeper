namespace MineSweeper.Domain;

public enum CellState
{
    Hidden,
    Revealed,
    Flagged,
    QuestionMarked,
    IsExploded,
    MineRevealed
}