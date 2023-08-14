using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a string from an integer.
/// </summary>
public partial class IntToStringConverter : Converter<IntToStringConverter>, IValueConverter
{
    [GeneratedRegex("[^0-9]")]
    private static partial Regex DigitReplaceRegex();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return 0;
        }
        if (value is not string str)
        {
            str = value.ToString();
            if (str == null)
            {
                return 0;
            }
        }

        str = DigitReplaceRegex().Replace(str, "");
        if (int.TryParse(str, out int result))
        {
            return result;
        }
        return 0;
    }
}
