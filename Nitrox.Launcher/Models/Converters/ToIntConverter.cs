using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Nitrox.Launcher.Models.Converters;

public class ToIntConverter : Converter<ToIntConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            return value switch
            {
                int i => i,
                string valueStr when int.TryParse(valueStr, out int result) => result,
                ICollection list => list.Count,
                IEnumerable enumerable => enumerable.Cast<object>().Count(),
                _ => System.Convert.ToInt32(value)
            };
        }
        catch
        {
            return 0;
        }
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
