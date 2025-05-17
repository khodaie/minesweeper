using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Messaging;

namespace MineSweeper.GameWindow;

sealed partial class GameControl
{
    private const int CellSize = 16;

    // Using a DependencyProperty as the backing store for RowsCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RowsCountProperty =
        DependencyProperty.Register(nameof(RowsCount), typeof(int), typeof(GameControl), new UIPropertyMetadata(0));

    public static readonly DependencyProperty ColumnsCountProperty =
        DependencyProperty.Register(nameof(ColumnsCount), typeof(int), typeof(GameControl), new PropertyMetadata(0));

    // Using a DependencyProperty as the backing store for MinesCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MinesCountProperty =
        DependencyProperty.Register(nameof(MinesCount), typeof(int), typeof(GameControl), new PropertyMetadata(0));

    public GameViewModel ViewModel { get; }

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

    public GameControl()
    {
        InitializeComponent();

        ViewModel = new GameViewModel(new GameInfo(16, 24, 16), WeakReferenceMessenger.Default);

        SetBinding(RowsCountProperty, new Binding(nameof(ViewModel.Board.RowsCount)) { Source = ViewModel });
        SetBinding(ColumnsCountProperty, new Binding(nameof(ViewModel.Board.ColumnsCount)) { Source = ViewModel });
        SetBinding(MinesCountProperty, new Binding(nameof(ViewModel.MinesCount)) { Source = ViewModel });
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
        GameGrid.ColumnDefinitions.Clear();
        GameGrid.RowDefinitions.Clear();
        GameGrid.Children.Clear();
        GameGrid.DataContext = ViewModel.Game;

        for (var j = 0; j < ViewModel.Board.ColumnsCount; j++)
        {
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = CellSize });
        }

        for (var i = 0; i < ViewModel.Board.RowsCount; i++)
        {
            GameGrid.RowDefinitions.Add(new RowDefinition { MinHeight = CellSize });
        }

        for (var i = 0; i < ViewModel.Board.RowsCount; i++)
        {
            for (var j = 0; j < ViewModel.Board.ColumnsCount; j++)
            {
                var cell = ViewModel.Board.GetCell(row: i, column: j);
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
        CreateBoard();

        WeakReferenceMessenger.Default.Register<GameWonMessage>(this, OnGameWon);
        WeakReferenceMessenger.Default.Register<GameOverMessage>(this, OnGameOver);
    }

    private void GameControl_OnUnloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<GameWonMessage>(this);
        WeakReferenceMessenger.Default.Unregister<GameOverMessage>(this);
    }
}