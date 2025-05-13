using System.Globalization;
using System.Windows.Data;
using MineSweeper.Domain;

namespace MineSweeper.GameWindow;

public sealed class CellContentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Cell cell)
            return null;

        return GetGameState() switch
        {
            GameState.Initializing => null,
            GameState.InProgress => GetDuringGameValue(cell),
            GameState.Won => GetWonValue(cell),
            GameState.GameOver => GetLostValue(cell),
            _ => throw new InvalidOperationException()
        };

        GameState? GetGameState()
        {
            if (parameter is not GameViewModel gameViewModel)
                return null;

            return gameViewModel.State;
        }
    }

    private static object? GetDuringGameValue(Cell cell) =>
        cell switch
        {
            { IsFlagged: true } => "🚩",
            { IsRevealed: false } => string.Empty,
            { NeighborMinesCount: > 0 } => cell.NeighborMinesCount.ToString(CultureInfo.CurrentUICulture),
            _ => (object?)null
        };

    private static object? GetWonValue(Cell cell) => cell switch
    {
        { IsFlagged: true } => "🚩",
        { IsRevealed: false, IsMine: false } => string.Empty,
        { IsRevealed: false, IsMine: true } => "🟩",
        { NeighborMinesCount: > 0 } => cell.NeighborMinesCount.ToString(CultureInfo.CurrentUICulture),
        _ => (object?)null
    };

    private static object? GetLostValue(Cell cell) => cell switch
    {
        { IsFlagged: true } => "🚩",
        { IsRevealed: false, IsMine: false } => string.Empty,
        { IsRevealed: false, IsMine: true } => "💣",
        { IsRevealed: true, IsMine: true } => "💥",
        { NeighborMinesCount: > 0 } => cell.NeighborMinesCount.ToString(CultureInfo.CurrentUICulture),
        _ => (object?)null
    };

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value;
}