using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Logging;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Service which prints out information at appropriate time in the app life cycle.
/// </summary>
internal sealed class StatusService(
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
    private readonly PlayerManager playerManager = playerManager;
    private readonly IOptions<ServerStartOptions> startOptions = startOptions;
    private readonly Func<ISummarize[]> summarizersProvider = summarizersProvider;
    private readonly IOptions<SubnauticaServerOptions> options = options;

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
        logger.ZLogInformation($"Server is listening on port {options.Value.ServerPort} UDP");
        logger.ZLogInformation($"Using {options.Value.SerializerMode} as save file serializer");
        logger.ZLogInformation($"Server Password: '{(string.IsNullOrWhiteSpace(options.Value.ServerPassword) ? "" : options.Value.ServerPassword):@password}'");
        logger.ZLogInformation($"Admin Password: '{(string.IsNullOrWhiteSpace(options.Value.AdminPassword) ? "" : options.Value.AdminPassword):@password}'");
        logger.ZLogInformation($"Autobackup: {(options.Value.MaxBackups < 1 ? "DISABLED" : $"ENABLED (Max: {options.Value.MaxBackups})")}");
        string summary = await GetServerSummary();
        logger.ZLogInformation($"Loaded save\n{summary}");
        await LogIps();

        async Task LogIps()
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

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        playerManager.SendPacketToAllPlayers(new ChatMessage(ChatMessage.SERVER_ID, "[BROADCAST] Server is shutting down..."));
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task<string> GetServerSummary(Perms viewerPerms = Perms.HOST)
    {
        StringBuilder builder = new("\n");
        List<string> summaries = [];
        foreach (ISummarize summarizer in summarizersProvider())
        {
            await foreach (string? summary in summarizer.GetSummaryAsync(viewerPerms))
            {
                summaries.Add(summary);
            }
        }
        foreach (string summary in summaries.OrderBy(s => s.Length))
        {
            builder.Append(" - ").Append(summary).AppendLine();
        }
        return builder.ToString();
    }

    public async IAsyncEnumerable<string> GetSummaryAsync(Perms viewerPerms)
    {
        if (viewerPerms >= Perms.HOST)
        {
            yield return await Task.FromResult($"Save location: {startOptions.Value.GetServerSavePath()}");
        }
        yield return await Task.FromResult($"Game mode: {options.Value.GameMode}");
    }
}
