using System.Windows;

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

    private void StartButton_OnClick(object sender, RoutedEventArgs e)
    {
        var window = new GameWindow.Game();
        window.Show();
    }
}