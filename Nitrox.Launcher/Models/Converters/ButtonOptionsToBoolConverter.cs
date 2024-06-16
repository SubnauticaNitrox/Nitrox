using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Nitrox.Launcher.ViewModels;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Returns true if the ButtonOptions values are equal to each other.
/// </summary>
public class ButtonOptionsToBoolConverter : Converter<EqualityConverter>, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ButtonOptions buttonOptions && parameter is string parameterString)
        {
            ButtonOptions parameterValue = Enum.Parse<ButtonOptions>(parameterString);
            return buttonOptions == parameterValue;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}