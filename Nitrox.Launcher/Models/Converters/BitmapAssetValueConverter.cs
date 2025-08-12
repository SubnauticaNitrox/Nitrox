using System;
using System.Globalization;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Utils;

namespace Nitrox.Launcher.Models.Converters;

public class BitmapAssetValueConverter : Converter<BitmapAssetValueConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            null => null,
            Bitmap when targetType.IsAssignableFrom(typeof(Bitmap)) => value,
            string s when targetType.IsAssignableFrom(typeof(Bitmap)) => AssetHelper.GetAssetFromStream(s, static stream => new Bitmap(stream)),
            _ => throw new NotSupportedException()
        };
}
