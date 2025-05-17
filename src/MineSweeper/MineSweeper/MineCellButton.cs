using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MineSweeper;

public sealed class MineCellButton : Button
{
    public static readonly DependencyProperty LeftClickCommandProperty =
        DependencyProperty.Register(nameof(LeftClickCommand), typeof(ICommand), typeof(MineCellButton));

    public static readonly DependencyProperty RightClickCommandProperty =
        DependencyProperty.Register(nameof(RightClickCommand), typeof(ICommand), typeof(MineCellButton));

    public ICommand? LeftClickCommand
    {
        get => (ICommand?)GetValue(LeftClickCommandProperty);
        set => SetValue(LeftClickCommandProperty, value);
    }

    public ICommand? RightClickCommand
    {
        get => (ICommand?)GetValue(RightClickCommandProperty);
        set => SetValue(RightClickCommandProperty, value);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);

        if (e.LeftButton == MouseButtonState.Pressed && LeftClickCommand?.CanExecute(CommandParameter) == true)
        {
            LeftClickCommand.Execute(CommandParameter);
            e.Handled = true;
        }
        else if (e.RightButton == MouseButtonState.Pressed && RightClickCommand?.CanExecute(CommandParameter) == true)
        {
            RightClickCommand.Execute(CommandParameter);
            e.Handled = true;
        }
    }
}