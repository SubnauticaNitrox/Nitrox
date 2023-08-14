using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Trims the value when retrieved by code but keeps the spaces in the input field intact for improved UX.
/// </summary>
/// <remarks>
///     This converter is unconventional (inverted converter) in that the value is converted for the backend.
///     The user wants to be able to input spaces while they're typing, but we don't want to save those spaces.
/// </remarks>
public class TrimConverter : Converter<TrimConverter>, IValueConverter
{
    /// <summary>
    ///     Cache to remember the last known untrimmed value (here, the value) for trimmed values (here, the key).
    /// </summary>
    private readonly Dictionary<string, string> inOutCache = new();

    /// <summary>
    ///     Converts trimmed value back to last known untrimmed value.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string strValue)
        {
            return value;
        }
        lock (inOutCache)
        {
            if (inOutCache.TryGetValue(strValue.Trim(), out string untrimmedValue))
            {
                strValue = untrimmedValue;
            }
        }
        return strValue;
    }

    /// <summary>
    ///     Converts untrimmed value back to trimmed value.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string strValue)
        {
            return value;
        }
        if (!strValue.StartsWith(' ') && !strValue.EndsWith(' '))
        {
            return strValue;
        }
        string trim = strValue.Trim();
        lock (inOutCache)
        {
            inOutCache[trim] = strValue;
        }
        return trim;
    }
}
