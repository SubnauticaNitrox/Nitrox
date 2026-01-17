using System.Buffers;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed class WriteConsoleColorLoggerMiddleware : ILoggerMiddleware
{
    public delegate (ConsoleColor? Foreground, ConsoleColor? Background) ColorSelect(ref ILoggerMiddleware.Context context);

    private static volatile int emitAnsiColorCodes = -1;

    public static bool CanEmitColors
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

    public required ColorSelect ColorSelector { get; set; } = (ref _) => (null, null);

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        (ConsoleColor? foreground, ConsoleColor? background) = ColorSelector(ref context);
        // Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
        if (background.HasValue)
        {
            context.Writer.Write(AnsiParser.GetBackgroundColorEscapeCode(background.Value));
        }
        if (foreground.HasValue)
        {
            context.Writer.Write(AnsiParser.GetForegroundColorEscapeCode(foreground.Value));
        }
        next(ref context);
        if (foreground.HasValue)
        {
            context.Writer.Write(AnsiParser.DefaultForegroundColor); // reset the foreground color
        }
        if (background.HasValue)
        {
            context.Writer.Write(AnsiParser.DefaultBackgroundColor); // reset the background color
        }
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
