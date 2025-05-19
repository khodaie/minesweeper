namespace MineSweeper.Domain;

public interface ICell
{
    CellState State { get; }
    Position Position { get; }
    bool IsMine { get; }
    sbyte NeighborMinesCount { get; }
    bool IsRevealed { get; }
    bool IsFlagged { get; }
    bool IsQuestionMarked { get; }
    bool IsExploded { get; }
    internal void Reveal();
    internal void ToggleFlag();
    internal void PlaceMine();
    internal void RevealMine();
    internal void IncreaseNeighborMinesCount();
}