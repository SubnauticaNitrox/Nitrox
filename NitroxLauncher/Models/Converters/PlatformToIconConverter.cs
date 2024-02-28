using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using NitroxModel.Discovery.Models;

namespace NitroxLauncher.Models.Converters
{
    [ValueConversion(typeof(Platform), typeof(string))]
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
                Platform.DISCORD => "pack://application:,,,/Assets/Images/store-icons/discord-2x.png",
                _ => "pack://application:,,,/Assets/Images/store-icons/missing-2x.png",
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        // We're inhereting from MarkupExtensions to avoid declaring this converter inside the ressources of a window
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
