using System.Windows;
using System.Windows.Controls;
using MineSweeper.GameWindow;

namespace MineSweeper;

sealed partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
    }

    private void StartGameButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: GameInfo gameInfo })
            return;

        CreateGame(gameInfo);
    }

    private void CreateGame(GameInfo gameInfo)
    {
        var window = new Game
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
        var window = sender as GameWindow.Game ?? throw new InvalidOperationException();
        window.Closed -= GameWindowOnClosed;
        Show();
        BringIntoView();
        Focus();
    }
}