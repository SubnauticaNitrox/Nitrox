using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Returns true if value is of the type as given by parameter (or any if parameter is a collection of types).
/// </summary>
public class IsTypeConverter : Converter<IsTypeConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (parameter)
        {
            case Type typeParameter:
                return typeParameter.IsInstanceOfType(value);
            case IEnumerable<Type> typeParameters:
            {
                foreach (Type type in typeParameters)
                {
                    if (type.IsInstanceOfType(value))
                    {
                        return true;
                    }
                }
                return false;
            }
            default:
                return new BindingNotification(new ArgumentException($"Expected {nameof(parameter)} to be a {typeof(Type).FullName}"), BindingErrorType.Error);
        }
    }
}
