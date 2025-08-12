using System;
using System.Globalization;
using Avalonia;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as "0 BoundValue 0 BoundValue" Margin from a Padding, used for TextBox styling.
/// </summary>
/// <remarks>
///     This converter is used to solve a niche issue with the styling of TextBoxes.
/// </remarks>
public class TextBoxPaddingToMarginConverter : Converter<TextBoxPaddingToMarginConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Thickness padding)
        {
            return value;
        }
        bool isNegative = parameter != null && bool.TryParse(parameter.ToString(), out bool result) && result;
        double top = isNegative ? -padding.Top : padding.Top;
        double bottom = isNegative ? -padding.Bottom : padding.Bottom;
        return new Thickness(0, top, 0, bottom);
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
