using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Returns true if values are equal to each other.
///     Or if value is singular, if parameter is equal to the value.
/// </summary>
public class EqualityConverter : Converter<EqualityConverter>, IMultiValueConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, parameter);

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();

    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        foreach (object val1 in values)
        {
            foreach (object val2 in values)
            {
                if (ReferenceEquals(val1, val2))
                {
                    continue;
                }
                if (!Equals(val1, val2))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
