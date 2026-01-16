using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Logging.Middleware;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;

namespace Nitrox.Server.Subnautica.Models.Logging;

/// <summary>
///     Standard log formatter used by Nitrox with preconfigured logging middleware based on
///     <see cref="NitroxFormatterOptions" />.
/// </summary>
internal sealed class NitroxZLoggerFormatter : MiddlewareZLoggerFormatter
{
    public NitroxZLoggerFormatter(NitroxFormatterOptions options)
    {
        Middleware = GetMiddleware(options).ToArray();
    }

    private static IEnumerable<ILoggerMiddleware> GetMiddleware(NitroxFormatterOptions options)
    {
        if (options.OmitWhenCaptured)
        {
            yield return new BreakLoggerMiddleware { BreakCondition = static (ref context) => context.Entry.TryGetProperty(out CaptureScope _) };
        }
        yield return new WriteLoggerMiddleware
        {
            Writer = static (ref context) =>
            {
                if (context.Entry.TryGetProperty(out PrefixScope scope))
                {
                    context.Writer.Write(scope.Prefix);
                }
            }
        };
        yield return new ConditionalGroupLoggerMiddleware
        {
            Condition = static context => !context.Entry.TryGetProperty(out PlainScope _),
            Group =
            [
                new WriteTimeLoggerMiddleware { Format = options.TimestampFormat ?? "" },
                new GroupLoggerMiddleware { Group = GetLogLevelMiddleware(options).ToArray() },
                new WriteLoggerMiddleware { Writer = static (ref context) => context.Writer.Write(" "u8) },
                new WriteCategoryLoggerMiddleware(),
                new WriteLoggerMiddleware { Writer = static (ref context) => context.Writer.Write(": "u8) },
            ]
        };
        if (options.UseRedaction)
        {
            yield return new WriteRedactedLogLoggerMiddleware { Redactors = options.Redactors };
        }
        else
        {
            yield return new WriteLogLoggerMiddleware();
        }
        yield return new WriteExceptionLoggerMiddleware();
        yield return new WriteNewLineLoggerMiddleware();
    }

    private static IEnumerable<ILoggerMiddleware> GetLogLevelMiddleware(NitroxFormatterOptions options)
    {
        if (options.ColorBehavior != LoggerColorBehavior.Disabled && WriteConsoleColorLoggerMiddleware.CanEmitColors)
        {
            yield return new WriteConsoleColorLoggerMiddleware
            {
                ColorSelector = (ref context) => context.Entry.LogInfo.LogLevel switch
                {
                    LogLevel.Trace => (ConsoleColor.Gray, ConsoleColor.Black),
                    LogLevel.Debug => (ConsoleColor.Gray, ConsoleColor.Black),
                    LogLevel.Information => (ConsoleColor.DarkGreen, ConsoleColor.Black),
                    LogLevel.Warning => (ConsoleColor.Yellow, ConsoleColor.Black),
                    LogLevel.Error => (ConsoleColor.Black, ConsoleColor.DarkRed),
                    LogLevel.Critical => (ConsoleColor.White, ConsoleColor.DarkRed),
                    _ => (null, null)
                }
            };
        }
        yield return new WriteLoggerMiddleware
        {
            Writer = (ref context) => context.Writer.Write(context.Entry.LogInfo.LogLevel switch
            {
                LogLevel.Trace => "[trce]"u8,
                LogLevel.Debug => "[dbug]"u8,
                LogLevel.Information => "[info]"u8,
                LogLevel.Warning => "[warn]"u8,
                LogLevel.Error => "[fail]"u8,
                LogLevel.Critical => "[crit]"u8,
                _ => throw new ArgumentOutOfRangeException(nameof(context.Entry.LogInfo.LogLevel))
            })
        };
    }
}
