using System;
using System.Globalization;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a double from a boolean.
///     When the value is true, it returns 1.0. When false, it returns 0.0.
/// </summary>
public class BoolToDoubleConverter : Converter<BoolToDoubleConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? 1.0 : 0.0;
        }
        return 0.0;
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
