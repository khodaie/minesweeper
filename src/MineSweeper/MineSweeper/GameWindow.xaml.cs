namespace MineSweeper;

sealed partial class GameWindow
{
    public GameWindow()
    {
        InitializeComponent();
    }

    public static readonly System.Windows.DependencyProperty RowsCountProperty =
        System.Windows.DependencyProperty.Register(
            nameof(RowsCount),
            typeof(int),
            typeof(GameWindow),
            new System.Windows.PropertyMetadata(0));

    public int RowsCount
    {
        get => (int)GetValue(RowsCountProperty);
        set => SetValue(RowsCountProperty, value);
    }

    public static readonly System.Windows.DependencyProperty ColumnsCountProperty =
        System.Windows.DependencyProperty.Register(
            nameof(ColumnsCount),
            typeof(int),
            typeof(GameWindow),
            new System.Windows.PropertyMetadata(0));

    public int ColumnsCount
    {
        get => (int)GetValue(ColumnsCountProperty);
        set => SetValue(ColumnsCountProperty, value);
    }

    public static readonly System.Windows.DependencyProperty MinesCountProperty =
        System.Windows.DependencyProperty.Register(
            nameof(MinesCount),
            typeof(int),
            typeof(GameWindow),
            new System.Windows.PropertyMetadata(0));

    public int MinesCount
    {
        get => (int)GetValue(MinesCountProperty);
        set => SetValue(MinesCountProperty, value);
    }
}
