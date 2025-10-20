using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Nitrox.Model.Constants;
using Nitrox.Model.MagicOnion;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Connects to locally running app that might want to track this server. Nitrox.Launcher is expected.
/// </summary>
internal sealed partial class ServerTrackingService(PlayerManager playerManager, IOptions<ServerStartOptions> options, ILogger<ServerTrackingService> logger) : BackgroundService
{
    [GeneratedRegex(@"\[(?<timestamp>\d{2}:\d{2}:\d{2}\.\d{3})\]\s\[(?<level>\w+)\]\s(?<category>\w+):\s(?<logText>(?:.|\n)*?(?=$|\n\[))")]
    private static partial Regex LogRegex { get; }

    public static readonly ConcurrentQueue<string> LogQueue = [];
    private readonly ILogger<ServerTrackingService> logger = logger;
    private readonly IOptions<ServerStartOptions> options = options;
    private GrpcChannel? channel;
    public override void Dispose() => channel?.Dispose();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        channel = GrpcChannel.ForAddress($"http://localhost:{LauncherConstants.LAUNCHER_SERVER_TRACK_PORT}");
        IServerTrackingService? api = MagicOnionClient.Create<IServerTrackingService>(channel);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await api.SetProcessId(Environment.ProcessId, options.Value.SaveName);
                await api.SetPlayerCount(Environment.ProcessId, playerManager.PlayerCount);
                while (LogQueue.TryDequeue(out string log) && LogRegex.Match(log) is { Success: true } match)
                {
                    int.TryParse(match.Groups["level"].Value, out int level);
                    await api.AddOutputLine(Environment.ProcessId, match.Groups["category"].Value, match.Groups["timestamp"].Value, level, match.Groups["logText"].Value);
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
            {
                // ignored
            }
            catch (Exception ex)
            {
                logger.ZLogTrace($"{ex.Message}");
            }
            await Task.Delay(5000, stoppingToken);
        }
    }
}
