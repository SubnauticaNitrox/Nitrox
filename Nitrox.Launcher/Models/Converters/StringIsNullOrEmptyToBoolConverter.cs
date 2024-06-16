using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Returns true if the input string value is null or empty.
/// </summary>
public class StringIsNullOrEmptyToBoolConverter : Converter<EqualityConverter>, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}