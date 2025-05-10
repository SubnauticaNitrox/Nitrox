using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which prints out information at appropriate time in the app life cycle.
/// </summary>
internal sealed class ServerStatusService([FromKeyedServices(typeof(ServerStatusService))] Stopwatch appStartStopWatch, GameInfo gameInfo, IOptions<ServerStartOptions> startOptions, IServerPacketSender packetSender, ILogger<ServerStatusService> logger)
    : IHostedLifecycleService
{
    private readonly GameInfo gameInfo = gameInfo;
    private readonly ILogger<ServerStatusService> logger = logger;
    private readonly IServerPacketSender packetSender = packetSender;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogServerStarting(NitroxEnvironment.ReleasePhase, NitroxEnvironment.Version, gameInfo.FullName);
        logger.LogGameInstallPathUsage(startOptions.Value.GameInstallPath);
        logger.LogSaveUsage(startOptions.Value.SaveName);
        return Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        appStartStopWatch.Stop();
        logger.ZLogInformation($"Server started in {Math.Round(appStartStopWatch.Elapsed.TotalSeconds, 3):@Seconds} seconds");

        await LogIps(cancellationToken);
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        packetSender.SendPacketToAll(new ChatMessage(SessionId.SERVER_ID, "[BROADCAST] Server is shutting down..."));
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task LogIps(CancellationToken cancellationToken)
    {
        Task<IPAddress> lanIp = Task.Run(NetHelper.GetLanIp, cancellationToken);
        Task<IPAddress> wanIp = NetHelper.GetWanIpAsync();
        Task<IPAddress> hamachiIp = Task.Run(NetHelper.GetHamachiIp, cancellationToken);
        await Task.WhenAll(lanIp, wanIp, hamachiIp);
        logger.ZLogInformation($"Use following IPs to connect");
        logger.ZLogInformation($"127.0.0.1 - You (Local)");
        if (wanIp.Result != null)
        {
            logger.LogWanIp(wanIp.Result);
        }
        if (hamachiIp.Result != null)
        {
            logger.LogHamachiIp(hamachiIp.Result);
        }
        // LAN IP could be null if all Ethernet/Wi-Fi interfaces are disabled.
        if (lanIp.Result != null)
        {
            logger.LogLanIp(lanIp.Result);
        }
    }
}
