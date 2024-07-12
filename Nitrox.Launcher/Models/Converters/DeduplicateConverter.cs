using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Removes duplicates by non-unique ToString values of the given list.
/// </summary>
public class DeduplicateConverter : Converter<DeduplicateConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<object> list)
        {
            return value;
        }

        return list.DistinctBy(i => i.ToString());
    }
}
