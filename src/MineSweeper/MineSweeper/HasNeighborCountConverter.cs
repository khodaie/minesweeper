using System.Globalization;
using System.Windows.Data;

namespace MineSweeper;

public sealed class HasNeighborCountConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is >= 1 and <= 9;

    object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}