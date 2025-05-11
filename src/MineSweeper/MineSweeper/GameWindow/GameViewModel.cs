using MineSweeper.Domain;

using System.Windows;

using DomainGame = MineSweeper.Domain.Game;

namespace MineSweeper.GameWindow;

public sealed class GameViewModel : DependencyObject
{
    private DomainGame? _game;

    public int RowsCount
    {
        get => (int)GetValue(RowsCountProperty);
        set => SetValue(RowsCountProperty, value);
    }

    public static readonly DependencyProperty RowsCountProperty =
        DependencyProperty.Register(nameof(RowsCount), typeof(int), typeof(GameViewModel), new PropertyMetadata(0));


    public int ColumnsCount
    {
        get => (int)GetValue(ColumnsCountProperty);
        set => SetValue(ColumnsCountProperty, value);
    }

    public static readonly DependencyProperty ColumnsCountProperty =
        DependencyProperty.Register(nameof(ColumnsCount), typeof(int), typeof(GameViewModel), new PropertyMetadata(0));



    public int MinesCount
    {
        get => (int)GetValue(MinesCountProperty);
        set => SetValue(MinesCountProperty, value);
    }
    public DomainGame Game => _game ?? throw new InvalidOperationException();

    // Using a DependencyProperty as the backing store for MinesCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MinesCountProperty =
        DependencyProperty.Register(nameof(MinesCount), typeof(int), typeof(GameViewModel), new PropertyMetadata(0));



    internal void InitializeGame()
    {
        _game = DomainGame.Create(RowsCount, ColumnsCount, MinesCount);
    }
}