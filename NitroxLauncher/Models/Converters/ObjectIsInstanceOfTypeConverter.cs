using System;
using System.Windows.Data;
using System.Windows.Markup;
using static NitroxModel.DisplayStatusCodes;
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
            DisplayStatusCode(StatusCode.INVALID_FUNCTION_CALL, "Operation not supported");
            return null; 
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
