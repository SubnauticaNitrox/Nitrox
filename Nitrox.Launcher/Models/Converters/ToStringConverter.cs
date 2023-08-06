using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a string using a specific formatting style.
/// </summary>
public class ToStringConverter : Converter<ToStringConverter>, IValueConverter
{
    private static readonly CultureInfo enUsCulture = new("en-US", false);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value?.GetType().IsEnum ?? false)
        {
            value = value.ToString();
        }
        if (value is not string sourceText || !targetType.IsAssignableTo(typeof(string)))
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
