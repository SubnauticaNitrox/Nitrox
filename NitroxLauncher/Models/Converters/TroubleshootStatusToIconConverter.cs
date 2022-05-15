using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Converters
{
    [ValueConversion(typeof(TroubleshootStatus), typeof(string))]
    internal class TroubleshootStatusToIconConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TroubleshootStatus status)
            {
                return "pack://application:,,,/Assets/Images/troubleshoot-icons/state-unknown-2x.png";
            }

            return status switch
            {
                TroubleshootStatus.NOT_STARTED => "pack://application:,,,/Assets/Images/troubleshoot-icons/state-pending-2x.png",
                TroubleshootStatus.RUNNING => "pack://application:,,,/Assets/Images/troubleshoot-icons/state-unknown-2x.png",
                TroubleshootStatus.OK => "pack://application:,,,/Assets/Images/troubleshoot-icons/state-ok-2x.png",
                TroubleshootStatus.KO => "pack://application:,,,/Assets/Images/troubleshoot-icons/state-ko-2x.png",
                TroubleshootStatus.FATAL_ERROR => "pack://application:,,,/Assets/Images/troubleshoot-icons/state-ko-2x.png",
                _ => "pack://application:,,,/Assets/Images/troubleshoot-icons/state-unknown-2x.png",
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
