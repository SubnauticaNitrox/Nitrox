using System;
using System.Globalization;
using Avalonia.Data;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Returns true if values are equal to each other.
///     Or if value is singular, if parameter is equal to the value.
/// </summary>
public class IsTypeConverter : Converter<IsTypeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not Type typeParameter)
        {
            return new BindingNotification(new ArgumentException($"Expected {nameof(parameter)} to be a {typeof(Type).FullName}"), BindingErrorType.Error);
        }

        return typeParameter.IsInstanceOfType(value);
    }
}
