using System;
using System.Net;
using Nitrox.Server.Subnautica.Core.Redaction;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Extensions;

internal static partial class LoggerExtensions
{
    [ZLoggerMessage(Level = LogLevel.Information, Message = "Starting Nitrox server {ReleasePhase} v{Version} for {GameName}")]
    public static partial void LogServerStarting(this ILogger logger, string releasePhase, Version version, string gameName);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Using game files from {path}")]
    public static partial void LogGameInstallPathUsage(this ILogger logger, SensitiveData<string> path);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Using world name {SaveName}")]
    public static partial void LogSaveUsage(this ILogger logger, string saveName);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{ip} - Friends on another internet network (Port Forwarding)")]
    public static partial void LogWanIp(this ILogger logger, SensitiveData<IPAddress> ip);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{ip} - Friends on same internet network (LAN)")]
    public static partial void LogLanIp(this ILogger logger, IPAddress ip);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{ip} - Friends using Hamachi (VPN)")]
    public static partial void LogHamachiIp(this ILogger logger, SensitiveData<IPAddress> ip);
    
    [ZLoggerMessage(Level = LogLevel.Trace, Message = "Adding {handler}")]
    public static partial void LogCommandHandlerAdded(this ILogger logger, CommandHandlerEntry handler);
}
