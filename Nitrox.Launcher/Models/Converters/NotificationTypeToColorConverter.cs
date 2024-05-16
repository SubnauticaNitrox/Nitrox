using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Converters;
using Nitrox.Launcher.Models.Extensions;

namespace Nitrox.Launcher.Models.Converters;

public class NotificationTypeToColorConverter : Converter<NotificationTypeToColorConverter>, IValueConverter
{
    private static readonly Dictionary<NotificationType, object> typeToResourceCache = new()
    {
        [NotificationType.Success] = Application.Current?.Resources.GetResource("BrandSuccessBrush"),
        [NotificationType.Information] = Application.Current?.Resources.GetResource("BrandInformationBrush"),
        [NotificationType.Warning] = Application.Current?.Resources.GetResource("BrandWarningBrush"),
        [NotificationType.Error] = Application.Current?.Resources.GetResource("BrandErrorBrush")
    };

    static NotificationTypeToColorConverter()
    {
        if (typeToResourceCache.Values.Any(t => t == null))
        {
            throw new Exception($"One or more notification types do not have an assigned color resource:{Environment.NewLine}{string.Join(", ", typeToResourceCache.Where(p => p.Value == null).Select(p => p.Key))}");
        }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
