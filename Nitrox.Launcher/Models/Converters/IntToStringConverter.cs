using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a string from an integer.
/// </summary>
public class IntToStringConverter : Converter<IntToStringConverter>, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return 0;
        }
        string str = value as string ?? value.ToString();

        str = Regex.Replace(str, "[^0-9]", "");
        if (int.TryParse(str, out int result))
        {
            return result;
        }
        return 0;
    }
}
