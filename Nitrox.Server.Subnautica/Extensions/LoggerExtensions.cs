using System.Net;
using Nitrox.Server.Subnautica.Models.Logging.Redaction;

namespace Nitrox.Server.Subnautica.Extensions;

internal static partial class LoggerExtensions
{
    [ZLoggerMessage(Level = LogLevel.Information, Message = "Starting Nitrox server {ReleasePhase} v{Version} for {GameName}")]
    public static partial void LogServerStarting(this ILogger logger, string releasePhase, Version version, string gameName);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Using game files from {path}")]
    public static partial void LogGamePath(this ILogger logger, SensitiveData<string> path);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "Using world name {SaveName}")]
    public static partial void LogSaveUsage(this ILogger logger, string saveName);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{ip} - Friends on another internet network (Port Forwarding)")]
    public static partial void LogWanIp(this ILogger logger, SensitiveData<IPAddress> ip);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{ip} - Friends on same internet network (LAN)")]
    public static partial void LogLanIp(this ILogger logger, IPAddress ip);

    [ZLoggerMessage(Level = LogLevel.Information, Message = "{ip} - Friends using {vpnName} (VPN)")]
    public static partial void LogVpnIp(this ILogger logger, string vpnName, SensitiveData<IPAddress> ip);
}
