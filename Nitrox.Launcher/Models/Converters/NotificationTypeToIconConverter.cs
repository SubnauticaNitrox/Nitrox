using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Nitrox.Launcher.Models.Converters;

public class NotificationTypeToIconConverter : Converter<NotificationTypeToIconConverter>
{
    private static readonly Dictionary<NotificationType, Bitmap> typeToResourceCache = new()
    {
        [NotificationType.Success] = new Bitmap(AssetLoader.Open(new Uri("avares://Nitrox.Launcher/Assets/Images/notification-icons/success.png"))),
        [NotificationType.Information] = new Bitmap(AssetLoader.Open(new Uri("avares://Nitrox.Launcher/Assets/Images/notification-icons/information.png"))),
        [NotificationType.Warning] = new Bitmap(AssetLoader.Open(new Uri("avares://Nitrox.Launcher/Assets/Images/notification-icons/warning.png"))),
        [NotificationType.Error] = new Bitmap(AssetLoader.Open(new Uri("avares://Nitrox.Launcher/Assets/Images/notification-icons/error.png")))
    };

    static NotificationTypeToIconConverter()
    {
        if (typeToResourceCache.Values.Any(t => t == null))
        {
            throw new Exception($"One or more notification types do not have an assigned icon resource:{Environment.NewLine}{string.Join(", ", typeToResourceCache.Where(p => p.Value == null).Select(p => p.Key))}");
        }
    }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Application.Current == null)
        {
            return null;
        }
        if (value is not NotificationType type)
        {
            return typeToResourceCache[NotificationType.Error];
        }

        return typeToResourceCache[type];
    }
}
