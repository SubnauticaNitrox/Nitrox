using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a string using a specific formatting style.
/// </summary>
public class ToStringConverter : BaseConverter<ToStringConverter>, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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
            case "SQL":
            case "sql":
                return sourceText.ToUpper();
            case "lower":
                return sourceText.ToLower();
            case "title":
            default:
                TextInfo txtinfo = new CultureInfo("en-US", false).TextInfo;
                return txtinfo.ToTitleCase(sourceText.ToLower().Replace("_", " "));
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}




