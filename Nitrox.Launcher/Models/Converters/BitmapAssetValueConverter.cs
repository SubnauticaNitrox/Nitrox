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
            not (null or string) => value.ToString(),
            string s when string.IsNullOrWhiteSpace(s) => parameter,
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
