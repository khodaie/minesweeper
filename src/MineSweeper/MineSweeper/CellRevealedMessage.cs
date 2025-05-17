namespace MineSweeper;

public sealed record GameOverMessage();

public sealed record GameWonMessage();

public sealed record CellRevealedMessage(CellViewModel Cell);

public sealed record CellToggleFlagMessage(CellViewModel Cell);

public sealed record RevealAdjacentCellsMessage(CellViewModel Cell);