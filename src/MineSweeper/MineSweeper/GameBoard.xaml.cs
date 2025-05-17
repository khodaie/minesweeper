using System.Windows;
using System.Windows.Controls;
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
        MessageBox.Show("You lost!", "Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);
        IsEnabled = false;
    }

    private void OnGameWon(object recipient, GameWonMessage message)
    {
        MessageBox.Show("You won!", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
        IsEnabled = false;
    }

    private void CreateBoard()
    {
        if (ViewModel is not { } viewModel)
            return;

        GameGrid.ColumnDefinitions.Clear();
        GameGrid.RowDefinitions.Clear();
        GameGrid.Children.Clear();
        GameGrid.DataContext = viewModel.Game;

        for (var j = 0; j < viewModel.Board.ColumnsCount; j++)
        {
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = CellSize });
        }

        for (var i = 0; i < viewModel.Board.RowsCount; i++)
        {
            GameGrid.RowDefinitions.Add(new RowDefinition { MinHeight = CellSize });
        }

        for (var i = 0; i < viewModel.Board.RowsCount; i++)
        {
            for (var j = 0; j < viewModel.Board.ColumnsCount; j++)
            {
                var cell = viewModel.Board.GetCell(row: i, column: j);
                var button = new CellButton
                {
                    ViewModel = cell
                };

                Grid.SetRow(button, i);
                Grid.SetColumn(button, j);
                GameGrid.Children.Add(button);
            }
        }
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