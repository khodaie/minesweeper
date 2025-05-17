using System.Windows;

namespace MineSweeper.GameWindow;

/// <summary>
/// Interaction logic for CellButton.xaml
/// </summary>
sealed partial class CellButton
{
    public CellButton()
    {
        InitializeComponent();
    }

    public CellViewModel ViewModel
    {
        get => (CellViewModel)GetValue(ViewModelProperty);
        init => SetValue(ViewModelProperty, value);
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(CellViewModel), typeof(CellButton),
            new UIPropertyMetadata(null));
}