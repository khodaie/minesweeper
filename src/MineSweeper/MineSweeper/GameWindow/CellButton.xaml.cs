using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using MineSweeper.Domain;

namespace MineSweeper.GameWindow;

/// <summary>
/// Interaction logic for CellButton.xaml
/// </summary>
sealed partial class CellButton
{
    public CellButton()
    {
        InitializeComponent();

#if DEBUG
        if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            // Mock Cell and ViewModel for design-time data
            var mockCell = Cell.CreateInstance(0, 0);

            ViewModel = new CellViewModel(mockCell, WeakReferenceMessenger.Default);
        }
#endif
    }

    public CellViewModel ViewModel
    {
        get => (CellViewModel)GetValue(ViewModelProperty);
        init => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(CellViewModel), typeof(CellButton),
            new UIPropertyMetadata(null));
}