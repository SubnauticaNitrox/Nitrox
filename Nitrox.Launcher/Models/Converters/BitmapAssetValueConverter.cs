using System;
using System.Globalization;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Utils;

namespace Nitrox.Launcher.Models.Converters;

public class BitmapAssetValueConverter : Converter<BitmapAssetValueConverter>
{
    public override object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        value = value switch
        {
            not string when parameter is string => parameter,
            string valueStr when string.IsNullOrWhiteSpace(valueStr) => parameter,
            not string and not null => value.ToString(),
            _ => value
        };

        return value switch
        {
            Bitmap when targetType.IsAssignableFrom(typeof(Bitmap)) => value,
            string s when targetType.IsAssignableFrom(typeof(Bitmap)) => AssetHelper.GetAssetFromStream(s, static stream => new Bitmap(stream)),
            _ => throw new NotSupportedException()
        };
    }
}
