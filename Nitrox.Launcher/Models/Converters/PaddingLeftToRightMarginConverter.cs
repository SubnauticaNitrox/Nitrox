using System;
using System.Globalization;
using Avalonia;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Maps a padding's left value to the right side of a margin.
/// </summary>
public class PaddingLeftToRightMarginConverter : Converter<PaddingLeftToRightMarginConverter>
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Thickness padding)
        {
            return new Thickness();
        }

        return new Thickness(0, 0, padding.Left, 0);
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
