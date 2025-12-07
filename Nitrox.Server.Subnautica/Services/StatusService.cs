using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which prints out information at appropriate time in the app life cycle.
/// </summary>
internal sealed partial class StatusService(
    [FromKeyedServices("startup")] Stopwatch appStartStopWatch,
    GameInfo gameInfo,
    PlayerManager playerManager,
    Func<ISummarize[]> summarizersProvider,
    IOptions<SubnauticaServerOptions> options,
    IOptions<ServerStartOptions> startOptions,
    ILogger<StatusService> logger) : IHostedLifecycleService, ISummarize
{
    private readonly GameInfo gameInfo = gameInfo;
    private readonly ILogger<StatusService> logger = logger;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly PlayerManager playerManager = playerManager;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly Func<ISummarize[]> summarizersProvider = summarizersProvider;

    [GeneratedRegex(@"\{([0-9]+)([^\}]*)\}")]
    private partial Regex FormatTagRegex { get; }

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
        logger.ZLogInformation($"Server started in {double.Round(appStartStopWatch.Elapsed.TotalSeconds, 3):@Seconds} seconds");
        logger.ZLogInformation($"Server is listening on port {options.Value.ServerPort} UDP");
        logger.ZLogInformation($"Using {options.Value.SerializerMode} as save file serializer");
        logger.ZLogInformation($"Server Password: '{(string.IsNullOrWhiteSpace(options.Value.ServerPassword) ? "" : options.Value.ServerPassword):@password}'");
        logger.ZLogInformation($"Admin Password: '{(string.IsNullOrWhiteSpace(options.Value.AdminPassword) ? "" : options.Value.AdminPassword):@password}'");
        logger.ZLogInformation($"Autobackup: {(options.Value.MaxBackups < 1 ? "DISABLED" : $"ENABLED (Max: {options.Value.MaxBackups})")}");
        logger.ZLogInformation($"Loaded save:");
        await LogServerSummary();
        await LogIps();

        async Task LogIps()
        {
            Task<IPAddress> lanIpTask = Task.Run(NetHelper.GetLanIp, cancellationToken);
            Task<IPAddress> wanIpTask = NetHelper.GetWanIpAsync();
            Task<IEnumerable<(IPAddress Address, string NetworkName)>> vpnIpsTasks = Task.Run(NetHelper.GetVpnIps, cancellationToken);
            await Task.WhenAll(lanIpTask, wanIpTask, vpnIpsTasks);
            logger.ZLogInformation($"Use IP to connect:");
            using (logger.BeginPlainScope())
            {
                using (logger.BeginPrefixScope("\t"))
                {
                    logger.ZLogInformation($"127.0.0.1 - You (Local)");
                    if (wanIpTask.Result is {} wanIp)
                    {
                        logger.LogWanIp(wanIp.TryExtractMappedIPv4());
                    }
                    foreach ((IPAddress? vpnAddress, string? vpnName) in await vpnIpsTasks)
                    {
                        logger.LogVpnIp(vpnName, vpnAddress);
                    }
                    // LAN IP could be null if all Ethernet/Wi-Fi interfaces are disabled.
                    if (lanIpTask.Result is {} lanIp)
                    {
                        logger.LogLanIp(lanIp);
                    }
                }
                logger.ZLogInformation($"");
            }
        }
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.ZLogInformation($"Server is stopping...");
        playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, "[BROADCAST] Server is shutting down..."));
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

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
                foreach (ISummarize summarizer in summarizersProvider())
                {
                    await summarizer.LogSummaryAsync(viewerPerms);
                }
            }
            logger.ZLogInformation($"");
        }
    }

    public Task LogSummaryAsync(Perms viewerPerms)
    {
        logger.ZLogInformation($"Game mode: {options.Value.GameMode}");
        if (viewerPerms >= Perms.HOST)
        {
            logger.ZLogInformation($"Save location: {startOptions.Value.GetServerSavePath():@Path}");
        }
        return Task.CompletedTask;
    }
}
