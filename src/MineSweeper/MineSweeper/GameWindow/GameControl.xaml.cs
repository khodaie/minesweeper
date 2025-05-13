using MineSweeper.Domain;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace MineSweeper.GameWindow;

public partial class GameControl
{
    private const int CellSize = 16;

    private Button[,]? _buttons;

    public GameViewModel GameViewModel => (GameViewModel)DataContext;

    public GameControl()
    {
        InitializeComponent();

        SetBinding(RowsCountProperty, new Binding(nameof(GameViewModel.RowsCount)) { Source = GameViewModel });
        SetBinding(ColumnsCountProperty, new Binding(nameof(GameViewModel.ColumnsCount)) { Source = GameViewModel });
        SetBinding(MinesCountProperty, new Binding(nameof(GameViewModel.MinesCount)) { Source = GameViewModel });
    }



    public int RowsCount
    {
        get { return (int)GetValue(RowsCountProperty); }
        set { SetValue(RowsCountProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RowsCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RowsCountProperty =
        DependencyProperty.Register(nameof(RowsCount), typeof(int), typeof(GameControl), new UIPropertyMetadata(0));

    public int ColumnsCount
    {
        get => (int)GetValue(ColumnsCountProperty);
        set => SetValue(ColumnsCountProperty, value);
    }

    public static readonly DependencyProperty ColumnsCountProperty =
        DependencyProperty.Register(nameof(ColumnsCount), typeof(int), typeof(GameControl), new PropertyMetadata(0));



    public int MinesCount
    {
        get => (int)GetValue(MinesCountProperty);
        set => SetValue(MinesCountProperty, value);
    }

    private Button[,] Buttons => _buttons ?? throw new InvalidOperationException();

    // Using a DependencyProperty as the backing store for MinesCount.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MinesCountProperty =
        DependencyProperty.Register(nameof(MinesCount), typeof(int), typeof(GameControl), new PropertyMetadata(0));

    private void CreateBoard()
    {
        GameGrid.ColumnDefinitions.Clear();
        GameGrid.RowDefinitions.Clear();
        GameGrid.Children.Clear();
        GameGrid.DataContext = GameViewModel.Game;

        for (int j = 0; j < GameViewModel.ColumnsCount; j++)
        {
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = CellSize });
        }
        for (int i = 0; i < GameViewModel.RowsCount; i++)
        {
            GameGrid.RowDefinitions.Add(new RowDefinition { MinHeight = CellSize });
        }

        _buttons = new Button[GameViewModel.RowsCount, GameViewModel.ColumnsCount];

        for (int i = 0; i < GameViewModel.RowsCount; i++)
        {
            for (int j = 0; j < GameViewModel.ColumnsCount; j++)
            {
                var button = new Button
                {
                    Content = string.Empty,
                    Background = System.Windows.Media.Brushes.LightGray,
                    DataContext = GameViewModel.Game.Board.GetCell(i, j)
                };

                button.PreviewMouseDown += Button_MouseDown;
                Grid.SetRow(button, i);
                Grid.SetColumn(button, j);
                GameGrid.Children.Add(button);
                _buttons[i, j] = button;
            }
        }
    }

    private void Button_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var button = (Button)sender;
        button.IsEnabled = false;
        Domain.CellViewModel cell = (Domain.CellViewModel)button.DataContext;

        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            cell.Reveal();
        }
        else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            cell.ToggleFlag();
        }

        DrawGame(cell.Board);

        if (cell.Board.State == GameState.GameOver)
        {
            MessageBox.Show("Game Over");
            IsEnabled = false;
        }
        else if (cell.Board.State == GameState.Won)
        {
            MessageBox.Show("You won!");
            IsEnabled = false;
        }
    }

    private void CreateGame()
    {
        GameViewModel.InitializeGame();
    }


    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        CreateGame();
        CreateBoard();
    }

    private void DrawGame(Board board)
    {
        for (int i = 0; i < GameViewModel.RowsCount; i++)
        {
            for (int j = 0; j < GameViewModel.ColumnsCount; j++)
            {
                var cell = board.GetCell(i, j);
                var button = Buttons[i, j];
                if (cell.IsRevealed)
                {
                    button.Background = System.Windows.Media.Brushes.White;
                    if (cell.IsMine)
                    {
                        button.Background = System.Windows.Media.Brushes.Red;
                        button.Foreground = System.Windows.Media.Brushes.Orange;
                        button.Content = "💣";
                    }
                    else
                    {
                        button.Content = cell.NeighborMinesCount == 0 ? string.Empty : cell.NeighborMinesCount.ToString();
                    }
                }
                else if (cell.IsFlagged)
                {
                    button.Content = "⚑";
                }
                else if (cell.Board.State == GameState.GameOver && cell.IsMine)
                {
                    if (!cell.IsRevealed)
                    {
                        button.Background = System.Windows.Media.Brushes.Yellow;
                        button.Foreground = System.Windows.Media.Brushes.DarkRed;
                        button.Content = "💥";
                    }
                    else
                    {
                        button.Background = System.Windows.Media.Brushes.White;
                        button.Foreground = System.Windows.Media.Brushes.Green;
                        button.Content = "💣";
                    }
                }
                else if (cell.Board.State == GameState.Won && cell.IsMine)
                {
                    button.Background = System.Windows.Media.Brushes.Green;
                    button.Foreground = System.Windows.Media.Brushes.Gray;
                    button.Content = "💣";
                }
                else
                {
                    button.Background = System.Windows.Media.Brushes.LightGray;
                    button.Content = string.Empty;
                }

            }
        }
    }
}