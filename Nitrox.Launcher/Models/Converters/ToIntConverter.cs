using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Nitrox.Launcher.Models.Converters;

public class ToIntConverter : Converter<ToIntConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            string valueStr when int.TryParse(valueStr, out int result) => result,
            ICollection list => list.Count,
            IEnumerable enumerable => enumerable.Cast<object>().Count(),
            _ => 0
        };

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
