using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Trims the value when retrieved by code but keeps the spaces in the input field intact for improved UX.
/// </summary>
/// <remarks>
///     This converter is unconventional (inverted converter) in that the value is converted for the backend.
///     The user wants to be able to input spaces while they're typing, but we don't want to save those spaces.
/// </remarks>
public class TrimConverter : Converter<TrimConverter>
{
    private readonly Lock inOutCacheLock = new();
    /// <summary>
    ///     Cache to remember the last known untrimmed value (here, the value) for trimmed values (here, the key).
    /// </summary>
    private readonly Dictionary<string, string> inOutCache = new();

    /// <summary>
    ///     Converts trimmed value back to last known untrimmed value.
    /// </summary>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string strValue)
        {
            return value;
        }
        lock (inOutCacheLock)
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
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string strValue)
        {
            return value;
        }
        if (!strValue.StartsWith(' ') && !strValue.EndsWith(' '))
        {
            // It's safe to reset cache now.
            lock (inOutCacheLock)
            {
                inOutCache.Clear();
            }
            return strValue;
        }
        string trim = strValue.Trim();
        lock (inOutCacheLock)
        {
            inOutCache[trim] = strValue;
        }
        return trim;
    }
}
