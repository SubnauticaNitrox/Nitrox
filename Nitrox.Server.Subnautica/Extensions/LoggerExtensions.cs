using System.Net;
using System.Runtime.CompilerServices;
using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Logging;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;

namespace Nitrox.Server.Subnautica.Extensions;

internal static partial class LoggerExtensions
{
    public static void ZLogWarningOnce(this ILogger logger,
                                       [InterpolatedStringHandlerArgument("logger")]
                                       ref DeduplicateWarningInterpolatedStringHandler message,
                                       object? context = null,
                                       [CallerMemberName] string? memberName = null,
                                       [CallerFilePath] string? filePath = null,
                                       [CallerLineNumber] int lineNumber = 0)
    {
        if (!message.ShouldLog())
        {
            return;
        }
        ZLoggerInterpolatedStringHandler zLogger = message.InnerHandler.InnerHandler;
        logger.ZLog(LogLevel.Warning, ref zLogger, context, memberName, filePath, lineNumber);
    }

    public static void ZLogErrorOnce(this ILogger logger,
                                     [InterpolatedStringHandlerArgument("logger")]
                                     ref DeduplicateErrorInterpolatedStringHandler message,
                                     object? context = null,
                                     [CallerMemberName] string? memberName = null,
                                     [CallerFilePath] string? filePath = null,
                                     [CallerLineNumber] int lineNumber = 0)
    {
        if (!message.ShouldLog())
        {
            return;
        }
        ZLoggerInterpolatedStringHandler zLogger = message.InnerHandler.InnerHandler;
        logger.ZLog(LogLevel.Error, null, ref zLogger, context, memberName, filePath, lineNumber);
    }

    public static void ZLogErrorOnce(this ILogger logger,
                                     Exception? exception,
                                     [InterpolatedStringHandlerArgument("logger")]
                                     ref DeduplicateErrorInterpolatedStringHandler message,
                                     object? context = null,
                                     [CallerMemberName] string? memberName = null,
                                     [CallerFilePath] string? filePath = null,
                                     [CallerLineNumber] int lineNumber = 0)
    {
        if (!message.ShouldLog())
        {
            return;
        }
        ZLoggerInterpolatedStringHandler zLogger = message.InnerHandler.InnerHandler;
        logger.ZLog(LogLevel.Error, exception, ref zLogger, context, memberName, filePath, lineNumber);
    }

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Starting Nitrox server {ReleasePhase} v{Version} for {GameName}")]
    public static partial void LogServerStarting(this ILogger logger, string releasePhase, Version version, string gameName);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Using game files from {Path}")]
    public static partial void LogGamePath(this ILogger logger, string path);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Using world name {SaveName}")]
    public static partial void LogSaveUsage(this ILogger logger, string saveName);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{IP} - Friends on another internet network (Port Forwarding)")]
    public static partial void LogWanIp(this ILogger logger, IPAddress ip);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{IP} - Friends on same internet network (LAN)")]
    public static partial void LogLanIp(this ILogger logger, IPAddress ip);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{IP} - Friends using {vpnName} (VPN)")]
    public static partial void LogVpnIp(this ILogger logger, string vpnName, IPAddress ip);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Opening directory {Path}")]
    public static partial void LogOpenDirectory(this ILogger logger, string path);

    [ZLoggerMessage(Level = LogLevel.Error, Message = "Unable to open directory {Path} because it does not exist")]
    public static partial void LogOpenDirectoryNotExists(this ILogger logger, string path);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Server password changed to '{Password}' by player '{PlayerName}'")]
    public static partial void LogServerPasswordChanged(this ILogger logger, string password, string playerName);

    [ZLoggerMessage(Level = LogLevel.Trace, Message = "Adding {Handler}")]
    public static partial void LogCommandHandlerAdded(this ILogger logger, CommandHandlerEntry handler);

    /// <summary>
    ///     Logs a save request as being issued by the issuer.
    /// </summary>
    /// <param name="logger">The logger instance to use.</param>
    /// <param name="name">Name of the issuer.</param>
    /// <param name="sessionId">Session ID of the issuer.</param>
    [ZLoggerMessage(Level = LogLevel.Information, Message = "Save requested by '{Name}' #{SessionId}")]
    public static partial void LogSaveRequest(this ILogger logger, string name, SessionId sessionId);

    extension(ILogger logger)
    {
        /// <summary>
        ///     Sets the logger into "plain" mode. Text will be logged without the time, category or log level info.
        /// </summary>
        public IDisposable? BeginPlainScope() => logger.BeginScope(new PlainScope());

        public IDisposable? BeginPrefixScope(string prefix) => logger.BeginScope(new PrefixScope(prefix));

        /// <inheritdoc cref="CaptureScope" />
        public CaptureScope BeginCaptureScope()
        {
            CaptureScope scope = new();
            IDisposable disposable = logger.BeginScope(scope);
            scope.InnerDisposable = disposable;
            return scope;
        }
    }
}
