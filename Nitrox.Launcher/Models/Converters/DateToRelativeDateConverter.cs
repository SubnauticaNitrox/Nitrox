using System;
using System.Globalization;

namespace Nitrox.Launcher.Models.Converters;

/// <summary>
///     Formats the bound value as a relative date string from a DateTime value.
/// </summary>
public class DateToRelativeDateConverter : Converter<DateToRelativeDateConverter>
{
    private const float DAYS_IN_YEAR = 365.2425f;
    private const float MEAN_DAYS_IN_MONTH = DAYS_IN_YEAR / 12f;

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DateTimeOffset date = value switch
        {
            DateTime dateTime => dateTime,
            DateTimeOffset dateTimeOffset => dateTimeOffset,
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            string text when DateTimeOffset.TryParse(text, out DateTimeOffset offset) => offset,
            _ => throw new ArgumentException($"Value must be a {nameof(DateTime)} or {nameof(DateTimeOffset)}", nameof(value))
        };

        TimeSpan delta = DateTimeOffset.UtcNow - date.UtcDateTime;

        return delta switch
        {
            { TotalSeconds: < 1 } => "just now",
            { TotalSeconds: < 2 } => "a second ago",
            { TotalMinutes: < 1 } => $"{(int)delta.TotalSeconds} seconds ago",
            { TotalMinutes: < 2 } => "a minute ago",
            { TotalMinutes: < 45 } => $"{(int)delta.TotalMinutes} minutes ago",
            { TotalHours: < 1.5 } => "an hour ago",
            { TotalDays: < 1 } => $"{(int)delta.TotalHours} hours ago",
            { TotalDays: < 2 } => "yesterday",
            { TotalDays: < MEAN_DAYS_IN_MONTH } => $"{(int)delta.TotalDays} days ago",
            { TotalDays: < MEAN_DAYS_IN_MONTH * 2 } => "a month ago",
            { TotalDays: < DAYS_IN_YEAR } => $"{(int)(delta.TotalDays / MEAN_DAYS_IN_MONTH)} months ago",
            { TotalDays: < DAYS_IN_YEAR * 2 } => "a year ago",
            _ => $"{(int)(delta.TotalDays / DAYS_IN_YEAR)} years ago"
        };
    }
}
