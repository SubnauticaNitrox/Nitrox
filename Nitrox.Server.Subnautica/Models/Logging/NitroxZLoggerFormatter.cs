using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;

namespace Nitrox.Server.Subnautica.Models.Logging;

internal partial class NitroxZLoggerFormatter : IZLoggerFormatter
{
    private static volatile int emitAnsiColorCodes = -1;

    private readonly ArrayBufferWriter<byte> redactionBufferWriter = new(256);

    [GeneratedRegex(@"\{[^\}]+\}", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking)]
    private partial Regex ParameterTagRegex { get; }

    public bool WithLineBreak => true;

    private static bool EmitAnsiColorCodes
    {
        get
        {
            int emitAnsi = emitAnsiColorCodes;
            if (emitAnsi != -1)
            {
                return Convert.ToBoolean(emitAnsi);
            }

            bool enabled = !Console.IsOutputRedirected;
            if (enabled)
            {
                enabled = Environment.GetEnvironmentVariable("NO_COLOR") is null;
            }
            else
            {
                string envVar = Environment.GetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION");
                enabled = envVar is not null && (envVar == "1" || envVar.Equals("true", StringComparison.OrdinalIgnoreCase));
            }

            emitAnsiColorCodes = Convert.ToInt32(enabled);
            return enabled;
        }
    }

    internal NitroxFormatterOptions FormatterOptions { get; init; }

    public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
    {
        writer.Write(GetEntryPrefix(entry));
        if (!IsPlainEntry(entry))
        {
            DateTimeOffset datetime = entry.LogInfo.Timestamp.Local;
            Span<byte> dateTimeDestination = writer.GetSpan();
            if (!datetime.TryFormat(dateTimeDestination, out int written, FormatterOptions.TimestampFormat))
            {
                datetime.TryFormat(dateTimeDestination, out written);
            }
            writer.Write(dateTimeDestination[..written]);

            ReadOnlySpan<byte> logLevelText = GetLogLevelText(entry.LogInfo.LogLevel);
            if (!logLevelText.IsEmpty)
            {
                ConsoleColors logLevelColors = GetLogLevelConsoleColors(entry.LogInfo.LogLevel);
                ConsoleColor? foreground = logLevelColors.Foreground;
                ConsoleColor? background = logLevelColors.Background;
                // Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
                if (background.HasValue)
                {
                    writer.Write(AnsiParser.GetBackgroundColorEscapeCode(background.Value));
                }
                if (foreground.HasValue)
                {
                    writer.Write(AnsiParser.GetForegroundColorEscapeCode(foreground.Value));
                }
                writer.Write(logLevelText);
                if (foreground.HasValue)
                {
                    writer.Write(AnsiParser.DefaultForegroundColor); // reset the foreground color
                }
                if (background.HasValue)
                {
                    writer.Write(AnsiParser.DefaultBackgroundColor); // reset the background color
                }
            }

            // category - if type name, truncate namespace.
            writer.Write(" "u8);
            if (entry.LogInfo.Category.Utf8Span.LastIndexOf("."u8) is var dotIndex and > -1)
            {
                writer.Write(entry.LogInfo.Category.Utf8Span[(dotIndex + 1) ..]);
            }
            else
            {
                writer.Write(entry.LogInfo.Category.Utf8Span);
            }
            writer.Write(": "u8);
        }

        if (!FormatterOptions.UseRedaction || !HasRedactableParameters(entry))
        {
            entry.ToString(writer);
        }
        else
        {
            // Get the original format which has the parameters as "{tag}".
            redactionBufferWriter.Clear();
            entry.WriteOriginalFormat(redactionBufferWriter);
            int bufferSize = Encoding.Default.GetCharCount(redactionBufferWriter.WrittenSpan);
            Span<char> originalFormat = bufferSize <= 64 ? stackalloc char[bufferSize] : new Span<char>();
            Encoding.UTF8.TryGetChars(redactionBufferWriter.WrittenSpan, originalFormat, out _);

            // Write out the log normally but handle redacted parameters when they occur.
            int paramIndex = 0;
            Range? lastMatch = null;
            foreach (ValueMatch match in ParameterTagRegex.EnumerateMatches(originalFormat))
            {
                // Write text before current match first.
                Range beforeRange = lastMatch ?? new Range(0, match.Index);
                if (!beforeRange.IsEmpty())
                {
                    writer.Write(originalFormat[beforeRange]);
                }

                // Write parameter value.
                writer.Write(TryGetRedactedValue(entry, paramIndex));

                lastMatch = new Range(match.Index, match.Index + match.Length);
                paramIndex++;
            }
            // Write last part of the log (text after last parameter value).
            if (lastMatch is { End: var val } && val.Value < originalFormat.Length - 1)
            {
                writer.Write(originalFormat[val..]);
            }
        }

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (entry.LogInfo.Exception is { } exception)
        {
            // exception message
            writer.Write("\n");
            writer.Write(exception.ToString());
        }
    }

    private static ReadOnlySpan<byte> GetLogLevelText(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => "[trce]"u8,
            LogLevel.Debug => "[dbug]"u8,
            LogLevel.Information => "[info]"u8,
            LogLevel.Warning => "[warn]"u8,
            LogLevel.Error => "[fail]"u8,
            LogLevel.Critical => "[crit]"u8,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };

    private bool TryGetProperty<T>(in IZLoggerEntry entry, out T result, T? equality = default)
    {
        if (entry.LogInfo.ScopeState is { } scope)
        {
            foreach (KeyValuePair<string, object?> keyValuePair in scope.Properties)
            {
                object value = keyValuePair.Value;
                if ((typeof(T).IsAssignableFrom(value.GetType()) && Equals(equality, default(T))) || value.Equals(equality))
                {
                    result = (T)value;
                    return true;
                }
            }
        }
        result = default;
        return false;
    }

    private bool IsPlainEntry(in IZLoggerEntry entry) => TryGetProperty(entry, out _, "plain");

    private ReadOnlySpan<char> GetEntryPrefix(in IZLoggerEntry entry)
    {
        if (TryGetProperty(entry, out Extensions.LoggerExtensions.PrefixScope value))
        {
            return value.Prefix.AsSpan();
        }
        return "";
    }

    private bool HasRedactableParameters(in IZLoggerEntry entry)
    {
        int parameterCount = entry.ParameterCount;
        for (int i = 0; i < parameterCount; i++)
        {
            if (FormatterOptions.GetRedactorsByKey(entry.GetParameterKeyAsString(i)).Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    private ReadOnlySpan<char> TryGetRedactedValue(IZLoggerEntry entry, int paramIndex)
    {
        string? value = entry.GetParameterValue(paramIndex)?.ToString() ?? "";
        List<IRedactor> redactors = FormatterOptions.GetRedactorsByKey(entry.GetParameterKeyAsString(paramIndex));
        if (redactors.Count < 1)
        {
            return value;
        }
        foreach (IRedactor redactor in redactors)
        {
            RedactResult result = redactor.Redact(value);
            if (!result.IsRedacted)
            {
                continue;
            }
            return result.Value;
        }
        return "<REDACTED>";
    }

    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        bool disableColors = FormatterOptions.ColorBehavior == LoggerColorBehavior.Disabled ||
                             (FormatterOptions.ColorBehavior == LoggerColorBehavior.Default && !EmitAnsiColorCodes);
        if (disableColors)
        {
            return new ConsoleColors(null, null);
        }
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
            _ => new ConsoleColors(null, null)
        };
    }

    private readonly struct ConsoleColors
    {
        public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
        {
            Foreground = foreground;
            Background = background;
        }

        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }
    }

    private static class AnsiParser
    {
        internal static ReadOnlySpan<byte> DefaultForegroundColor => "\e[39m\e[22m"u8; // reset to default foreground color
        internal static ReadOnlySpan<byte> DefaultBackgroundColor => "\e[49m"u8; // reset to the background color

        internal static ReadOnlySpan<byte> GetForegroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\e[30m"u8,
                ConsoleColor.DarkRed => "\e[31m"u8,
                ConsoleColor.DarkGreen => "\e[32m"u8,
                ConsoleColor.DarkYellow => "\e[33m"u8,
                ConsoleColor.DarkBlue => "\e[34m"u8,
                ConsoleColor.DarkMagenta => "\e[35m"u8,
                ConsoleColor.DarkCyan => "\e[36m"u8,
                ConsoleColor.Gray => "\e[37m"u8,
                ConsoleColor.Red => "\e[1m\e[31m"u8,
                ConsoleColor.Green => "\e[1m\e[32m"u8,
                ConsoleColor.Yellow => "\e[1m\e[33m"u8,
                ConsoleColor.Blue => "\e[1m\e[34m"u8,
                ConsoleColor.Magenta => "\e[1m\e[35m"u8,
                ConsoleColor.Cyan => "\e[1m\e[36m"u8,
                ConsoleColor.White => "\e[1m\e[37m"u8,
                _ => DefaultForegroundColor // default foreground color
            };

        internal static ReadOnlySpan<byte> GetBackgroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\e[40m"u8,
                ConsoleColor.DarkRed => "\e[41m"u8,
                ConsoleColor.DarkGreen => "\e[42m"u8,
                ConsoleColor.DarkYellow => "\e[43m"u8,
                ConsoleColor.DarkBlue => "\e[44m"u8,
                ConsoleColor.DarkMagenta => "\e[45m"u8,
                ConsoleColor.DarkCyan => "\e[46m"u8,
                ConsoleColor.Gray => "\e[47m"u8,
                _ => DefaultBackgroundColor // Use default background color
            };
    }
}
