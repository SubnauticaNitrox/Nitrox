using System;
using System.Globalization;
using Avalonia.Data.Converters;
using NitroxModel.Discovery.Models;

namespace Nitrox.Launcher.Models.Converters;

internal class PlatformToIconConverter : Converter<PlatformToIconConverter>, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return BitmapAssetValueConverter.GetBitmapFromPath(GetIconPathForPlatform(value as Platform?));
    }

    private string GetIconPathForPlatform(Platform? platform)
    {
        return platform switch
        {
            Platform.EPIC => "/Assets/Images/store-icons/epic-2x.png",
            Platform.STEAM => "/Assets/Images/store-icons/steam-2x.png",
            Platform.MICROSOFT => "/Assets/Images/store-icons/xbox-2x.png",
            Platform.DISCORD => "/Assets/Images/store-icons/discord-2x.png",
            _ => "/Assets/Images/store-icons/missing-2x.png",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
