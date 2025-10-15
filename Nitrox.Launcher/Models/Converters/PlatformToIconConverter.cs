using System;
using System.Globalization;
using Avalonia.Media.Imaging;
using Nitrox.Launcher.Models.Utils;
using NitroxModel.Platforms.Discovery.Models;

namespace Nitrox.Launcher.Models.Converters;

public class PlatformToIconConverter : Converter<PlatformToIconConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return AssetHelper.GetAssetFromStream(GetIconPathForPlatform(value as Platform?), static stream => new Bitmap(stream));
    }

    private static string GetIconPathForPlatform(Platform? platform) => $"/Assets/Images/store-icons/{platform switch
    {
        Platform.STEAM => "steam.png",
        Platform.EPIC => "epic.png",
        Platform.HEROIC => "heroic.png",
        Platform.MICROSOFT => "xbox.png",
        Platform.DISCORD => "discord.png",
        _ => "missing.png",
    }}";
}
