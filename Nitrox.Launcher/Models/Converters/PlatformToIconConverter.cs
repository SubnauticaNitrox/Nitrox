using System;
using System.Globalization;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Model.Platforms.Discovery.Models;

namespace Nitrox.Launcher.Models.Converters;

public class PlatformToIconConverter : Converter<PlatformToIconConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return AssetHelper.GetAssetFromStream(GetIconPathForPlatform(value as Platform?), static stream => new Bitmap(stream));
    }

    private static string GetIconPathForPlatform(Platform? platform) => platform switch
    {
        Platform.STEAM => "/Assets/Images/store-icons/steam.png",
        Platform.EPIC => "/Assets/Images/store-icons/epic.png",
        Platform.HEROIC => "/Assets/Images/store-icons/heroic.png",
        Platform.MICROSOFT => "/Assets/Images/store-icons/xbox.png",
        Platform.DISCORD => "/Assets/Images/store-icons/discord.png",
        _ => "/Assets/Images/store-icons/missing.png",
    };
}
