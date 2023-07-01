using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace NitroxLauncher.Models.Converters;

[ValueConversion(typeof(DateTime), typeof(string))]
internal class DateToRelativeDateConverter : MarkupExtension, IValueConverter
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

        return delta switch
        {
            < 1 * MINUTE => ts.Seconds == 1 ? "one second ago" : $"{ts.Seconds} seconds ago",
            < 2 * MINUTE => "a minute ago",
            < 45 * MINUTE => $"{ts.Minutes} minutes ago",
            < 90 * MINUTE => "an hour ago",
            < 24 * HOUR => $"{ts.Hours} hours ago",
            < 48 * HOUR => "yesterday",
            < 30 * DAY => $"{ts.Days} days ago",
            < 12 * MONTH => $"{System.Convert.ToInt32(Math.Floor((double)ts.Days / 30))} month(s) ago",
            _ => $"{System.Convert.ToInt32(Math.Floor((double)ts.Days / 365))} years ago"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    // We're inhereting from MarkupExtensions to avoid declaring this converter inside the ressources of a window
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
