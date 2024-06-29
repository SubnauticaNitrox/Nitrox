using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Data;
using Avalonia.Data.Converters;
using NitroxModel;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a string using a specific formatting style.
/// </summary>
public class ToStringConverter : Converter<ToStringConverter>, IValueConverter
{
    private static readonly CultureInfo enUsCulture = new("en-US", false);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        if (value.GetType().IsEnum)
        {
            value = (value as Enum)?.GetAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
        }
        if (value is not string sourceText)
        {
            sourceText = value?.ToString();
        }
        if (!targetType.IsAssignableTo(typeof(string)) || sourceText == null)
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        switch (parameter)
        {
            case "upper":
                return sourceText.ToUpperInvariant();
            case "lower":
                return sourceText.ToLowerInvariant();
            default:
                return enUsCulture.TextInfo.ToTitleCase(sourceText.ToLower().Replace("_", " "));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
