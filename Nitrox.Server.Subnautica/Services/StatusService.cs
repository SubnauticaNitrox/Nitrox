using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which prints out information at an appropriate time in the app life cycle.
/// </summary>
internal sealed class StatusService(
    [FromKeyedServices("startup")] Stopwatch appStartStopWatch,
    GameInfo gameInfo,
    IPacketSender packetSender,
    ISummarize.Trigger summarizeTrigger,
    IOptions<SubnauticaServerOptions> options,
    IOptions<ServerStartOptions> startOptions,
    ILogger<StatusService> logger) : IHostedLifecycleService, ISummarize
{
    private readonly GameInfo gameInfo = gameInfo;
    private readonly ILogger<StatusService> logger = logger;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly IPacketSender packetSender = packetSender;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly ISummarize.Trigger summarizeTrigger = summarizeTrigger;

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogServerStarting(NitroxEnvironment.ReleasePhase, NitroxEnvironment.Version, gameInfo.FullName);
        logger.LogGamePath(startOptions.Value.GamePath ?? "unknown");
        logger.LogSaveUsage(startOptions.Value.SaveName);
        return Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        appStartStopWatch.Stop();
        SubnauticaServerOptions o = options.Value;
        logger.ZLogInformation($"Server started in {double.Round(appStartStopWatch.Elapsed.TotalSeconds, 3):@Seconds} seconds");
        logger.ZLogInformation($"Server is listening on port {o.ServerPort} UDP");
        logger.ZLogInformation($"Using {o.SerializerMode} as save file serializer");
        logger.ZLogInformation($"Server Password: '{(string.IsNullOrWhiteSpace(o.ServerPassword) ? "" : o.ServerPassword):@password}'");
        logger.ZLogInformation($"Admin Password: '{(string.IsNullOrWhiteSpace(o.AdminPassword) ? "" : o.AdminPassword):@password}'");
        logger.ZLogInformation($"Autobackup: {(o.MaxBackups < 1 ? "DISABLED" : $"ENABLED (Max: {o.MaxBackups})")}");
        logger.ZLogInformation($"Loaded save:");
        await LogServerSummary();
        await LogIpsAsync();
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.ZLogInformation($"Server is stopping...");
        packetSender.SendPacketToAllAsync(new ChatMessage(SessionId.SERVER_ID, "[BROADCAST] Server is shutting down..."));
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task LogIpsAsync()
    {
        await using (logger.BeginAtomicScope())
        using (logger.BeginPlainScope())
        {
            logger.ZLogInformation($"Use IP to connect:");
            using (logger.BeginPrefixScope("\t"))
            {
                logger.ZLogInformation($"{IPAddress.Loopback} - You (Local)");
                foreach ((IPAddress address, NetHelper.MachineIpOrigin origin, string? networkName) in await NetHelper.GetAllKnownIpsAsync())
                {
                    switch (origin)
                    {
                        case NetHelper.MachineIpOrigin.LAN:
                            logger.LogLanIp(address);
                            break;
                        case NetHelper.MachineIpOrigin.VPN:
                            logger.LogVpnIp(networkName!, address);
                            break;
                        case NetHelper.MachineIpOrigin.WAN:
                            logger.LogWanIp(address);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Logs a user-friendly summary of the entire server state.
    /// </summary>
    public async Task LogServerSummary(Perms viewerPerms = Perms.HOST)
    {
        using (logger.BeginPlainScope())
        {
            logger.ZLogInformation($"");
            using (logger.BeginPrefixScope(" - "))
            {
                await summarizeTrigger.InvokeAsync(new(viewerPerms));
            }
            logger.ZLogInformation($"");
        }
    }

    Task IEvent<ISummarize.Args>.OnEventAsync(ISummarize.Args args)
    {
        logger.ZLogInformation($"Game mode: {options.Value.GameMode}");
        if (args.ViewerPerms >= Perms.HOST)
        {
            logger.ZLogInformation($"Save location: {startOptions.Value.GetServerSavePath():@Path}");
        }
        return Task.CompletedTask;
    }
}
