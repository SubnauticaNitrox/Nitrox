using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which prints out information at appropriate time in the app life cycle.
/// </summary>
internal sealed class StatusService([FromKeyedServices("startup")] Stopwatch appStartStopWatch, GameInfo gameInfo, PlayerManager playerManager, IOptions<ServerStartOptions> startOptions, ILogger<StatusService> logger)
    : IHostedLifecycleService
{
    private readonly GameInfo gameInfo = gameInfo;
    private readonly ILogger<StatusService> logger = logger;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly PlayerManager playerManager = playerManager;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogServerStarting(NitroxEnvironment.ReleasePhase, NitroxEnvironment.Version, gameInfo.FullName);
        logger.LogGamePath(startOptions.Value.GamePath);
        logger.LogSaveUsage(startOptions.Value.SaveName);
        return Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        appStartStopWatch.Stop();
        logger.ZLogInformation($"Server started in {double.Round(appStartStopWatch.Elapsed.TotalSeconds, 3):@Seconds} seconds");
        await LogIps(cancellationToken);
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, "[BROADCAST] Server is shutting down..."));
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public string GetSummary(Perms viewerPerms = Perms.CONSOLE)
    {
        // TODO: Extend summary with more useful save file data
        StringBuilder builder = new("\n");
        if (viewerPerms is Perms.CONSOLE)
        {
            builder.AppendLine($" - Save location: {startOptions.Value.GetServerSavePath()}");
        }
        // TODO: FIX!
//         builder.AppendLine($"""
//                              - Aurora's state: {storyManager.GetAuroraStateSummary()}
//                              - Current time: day {timeKeeper.Day} ({Math.Floor(timeKeeper.ElapsedSeconds)}s)
//                              - Scheduled goals stored: {world.GameData.StoryGoals.ScheduledGoals.Count}
//                              - Story goals completed: {world.GameData.StoryGoals.CompletedGoals.Count}
//                              - Radio messages stored: {world.GameData.StoryGoals.RadioQueue.Count}
//                              - World gamemode: {options.Value.GameMode}
//                              - Encyclopedia entries: {world.GameData.PDAState.EncyclopediaEntries.Count}
//                              - Known tech: {world.GameData.PDAState.KnownTechTypes.Count}
//                             """);

        return builder.ToString();
    }

    private async Task LogIps(CancellationToken cancellationToken)
    {
        Task<IPAddress> lanIp = Task.Run(NetHelper.GetLanIp, cancellationToken);
        Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();
        Task<IEnumerable<(IPAddress Address, string NetworkName)>> vpnIps = Task.Run(NetHelper.GetVpnIps, cancellationToken);
        await Task.WhenAll(lanIp, wanIp, vpnIps);
        logger.ZLogInformation($"Use following IPs to connect");
        logger.ZLogInformation($"127.0.0.1 - You (Local)");
        if (wanIp.Result != null)
        {
            logger.LogWanIp(wanIp.Result);
        }
        foreach ((IPAddress? vpnAddress, string? vpnName) in await vpnIps)
        {
            logger.LogVpnIp(vpnName, vpnAddress);
        }
        // LAN IP could be null if all Ethernet/Wi-Fi interfaces are disabled.
        if (lanIp.Result != null)
        {
            logger.LogLanIp(lanIp.Result);
        }
    }
}
