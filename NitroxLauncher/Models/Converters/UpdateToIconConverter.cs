using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace NitroxLauncher.Models.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    internal class UpdateToIconConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolean)
            {
                return "pack://application:,,,/Images/material-design-icons/download.png";
            }

            return boolean switch
            {
                true => "pack://application:,,,/Images/material-design-icons/downloadDot.png",
                false => "pack://application:,,,/Images/material-design-icons/download.png"
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
