using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace NitroxLauncher.Models.Converters
{
    [ValueConversion(typeof(Type), typeof(bool))]
    public class ObjectIsInstanceOfTypeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            if (parameter == null)
            {
                return false;
            }
            Type valueType = value as Type ?? value.GetType();
            Type parameterType = parameter as Type ?? parameter.GetType();
            return parameterType.IsAssignableFrom(valueType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
