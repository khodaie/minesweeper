using System.Windows;
using System.Windows.Controls;

namespace MineSweeper;

sealed partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void StartGameButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: GameInfo gameInfo })
            return;

        CreateGame(gameInfo);
    }

    private void CreateGame(GameInfo gameInfo)
    {
        var window = new GameWindow
        {
            RowsCount = gameInfo.RowsCount,
            ColumnsCount = gameInfo.ColumnsCount,
            MinesCount = gameInfo.MinesCount
        };

        window.Closed += GameWindowOnClosed;
        Hide();
        window.Show();
    }

    private void GameWindowOnClosed(object? sender, EventArgs e)
    {
        var window = sender as GameWindow ?? throw new InvalidOperationException();
        window.Closed -= GameWindowOnClosed;
        Show();
        BringIntoView();
        Focus();
    }
}