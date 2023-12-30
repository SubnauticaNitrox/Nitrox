using System;
using System.Globalization;
using NitroxModel.Discovery;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Nitrox.Launcher.Models.Converters;

internal class PlatformToIconConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Platform platform)
        {
            return "pack://application:,,,/Assets/Images/store-icons/missing-2x.png";
        }

        return platform switch
        {
            Platform.EPIC => "pack://application:,,,/Assets/Images/store-icons/epic-2x.png",
            Platform.STEAM => "pack://application:,,,/Assets/Images/store-icons/steam-2x.png",
            Platform.MICROSOFT => "pack://application:,,,/Assets/Images/store-icons/xbox-2x.png",
            Platform.PIRATED => "pack://application:,,,/Assets/Images/store-icons/pirated-2x.png",
            Platform.DISCORD => "pack://application:,,,/Assets/Images/store-icons/discord-2x.png",
            _ => "pack://application:,,,/Assets/Images/store-icons/missing-2x.png",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    // We're inheriting from MarkupExtensions to avoid declaring this converter inside the resources of a window
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}