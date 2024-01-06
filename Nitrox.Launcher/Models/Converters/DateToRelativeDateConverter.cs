using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a relative date string from a DateTime value.
/// </summary>
public partial class DateToRelativeDateConverter : Converter<DateToRelativeDateConverter>, IValueConverter
{
    private const int SECOND = 1;
    private const int MINUTE = 60 * SECOND;
    private const int HOUR = 60 * MINUTE;
    private const int DAY = 24 * HOUR;
    private const int MONTH = 30 * DAY;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime date)
        {
            return string.Empty;
        }

        TimeSpan ts = new(DateTime.UtcNow.Ticks - date.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        switch (delta)
        {
            case < 1 * MINUTE:
                return ts.Seconds == 1 ? "one second ago" : $"{ts.Seconds} seconds ago";

            case < 2 * MINUTE:
                return "a minute ago";

            case < 45 * MINUTE:
                return $"{ts.Minutes} minutes ago";

            case < 90 * MINUTE:
                return "an hour ago";

            case < 24 * HOUR:
                return $"{ts.Hours} hours ago";

            case < 48 * HOUR:
                return "yesterday";

            case < 30 * DAY:
                return $"{ts.Days} days ago";

            case < 12 * MONTH:
            {
                int months = System.Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : $"{months} months ago";
            }

            default:
            {
                int years = System.Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : $"{years} years ago";
            }

        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
