using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace MineSweeper;

sealed partial class GameBoard
{
    private const int CellSize = 16;

    public static readonly DependencyProperty RowsCountProperty =
        DependencyProperty.Register(nameof(RowsCount), typeof(int), typeof(GameBoard),
            new UIPropertyMetadata(0, static (s, _) => ((GameBoard)s).Render()));

    public static readonly DependencyProperty ColumnsCountProperty =
        DependencyProperty.Register(nameof(ColumnsCount), typeof(int), typeof(GameBoard),
            new PropertyMetadata(0, static (s, _) => ((GameBoard)s).Render()));

    public static readonly DependencyProperty MinesCountProperty =
        DependencyProperty.Register(nameof(MinesCount), typeof(int), typeof(GameBoard),
            new PropertyMetadata(0, static (s, _) => ((GameBoard)s).Render()));

    public GameViewModel? ViewModel
    {
        get => (GameViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(GameViewModel), typeof(GameBoard),
            new UIPropertyMetadata(defaultValue: null));

    public int RowsCount
    {
        get => (int)GetValue(RowsCountProperty);
        set => SetValue(RowsCountProperty, value);
    }

    public int ColumnsCount
    {
        get => (int)GetValue(ColumnsCountProperty);
        set => SetValue(ColumnsCountProperty, value);
    }

    public int MinesCount
    {
        get => (int)GetValue(MinesCountProperty);
        set => SetValue(MinesCountProperty, value);
    }

    public GameBoard()
    {
        InitializeComponent();
    }

    private void Render()
    {
        if (RowsCount <= 0 || ColumnsCount <= 0 || MinesCount <= 0)
        {
            ViewModel = null;
            return;
        }

        ViewModel = new GameViewModel(new GameInfo(RowsCount, ColumnsCount, MinesCount),
            WeakReferenceMessenger.Default);

        CreateBoard();
    }

    private void OnGameOver(object recipient, GameOverMessage message)
    {
        MessageBox.Show("💥 Boom! You hit a mine! 💀 Better luck next time! 💣", "Game Over", MessageBoxButton.OK,
            MessageBoxImage.Warning);
        IsEnabled = false;
    }

    private void OnGameWon(object recipient, GameWonMessage message)
    {
        MessageBox.Show("🎉 Congratulations! You cleared the minefield and won the game! 🏆", "Game Over",
            MessageBoxButton.OK, MessageBoxImage.Information);
        IsEnabled = false;
    }

    private void CreateBoard()
    {
        if (ViewModel is not { } viewModel)
            return;

        var mainWindow = Application.Current.MainWindow;
        if (mainWindow is null)
            return;

        mainWindow.Cursor = Cursors.Wait;
        try
        {
            var grid = CreateGameGrid(viewModel);

            AddChild(grid);
        }
        finally
        {
            mainWindow.Cursor = Cursors.Arrow;
        }
    }

    private Grid CreateGameGrid(GameViewModel viewModel)
    {
        var grid = new Grid();

        for (var i = 0; i < RowsCount; i++)
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });

        for (var j = 0; j < ColumnsCount; j++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });

        grid.DataContext = viewModel.Game;

        var children = grid.Children;

        children.Capacity = RowsCount * ColumnsCount;

        for (var i = 0; i < RowsCount; i++)
        {
            for (var j = 0; j < ColumnsCount; j++)
            {
                var cell = viewModel.Board.GetCell(i, j);
                var button = new CellButton
                {
                    ViewModel = cell
                };
                Grid.SetRow(button, i);
                Grid.SetColumn(button, j);
                children.Add(button);
            }
        }

        return grid;
    }

    private void GameControl_Loaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Register<GameWonMessage>(this, OnGameWon);
        WeakReferenceMessenger.Default.Register<GameOverMessage>(this, OnGameOver);
    }

    private void GameControl_OnUnloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<GameWonMessage>(this);
        WeakReferenceMessenger.Default.Unregister<GameOverMessage>(this);
    }
}