using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Data;
using NitroxModel.Helper;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a string using a specific formatting style.
/// </summary>
public class ToStringConverter : Converter<ToStringConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
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

        return parameter switch
        {
            "upper" => sourceText.ToUpperInvariant(),
            "lower" => sourceText.ToLowerInvariant(),
            _ => CultureManager.CultureInfo.TextInfo.ToTitleCase(sourceText.ToLower().Replace("_", " ")),
        };
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
}
