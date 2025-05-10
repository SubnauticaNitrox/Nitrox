using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mono.Nat;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Helper;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Opens ports on network attached routers via <a href="https://en.wikipedia.org/wiki/Universal_Plug_and_Play">UPnP</a>.
/// </summary>
/// <remarks>
///     By port forwarding, incoming connections will be forwarded to the host machine running the game server.<br /><br />
///     <b>Trivia</b>: Routers are by default configured to block incoming connections coming from WAN, for security.
/// </remarks>
internal class PortForwardService(IOptionsMonitor<SubnauticaServerOptions> optionsProvider, ILogger<PortForwardService> logger) : BackgroundService
{
    private readonly ILogger<PortForwardService> logger = logger;
    private readonly ConcurrentDictionary<ushort, bool> openedPorts = [];
    private readonly IOptionsMonitor<SubnauticaServerOptions> optionsProvider = optionsProvider;

    private readonly Channel<PortForwardAction> portForwardChannel = Channel.CreateBounded<PortForwardAction>(new BoundedChannelOptions(10)
    {
        AllowSynchronousContinuations = false,
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true,
        SingleWriter = true
    });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ListenForOptionsChange();
        await QueueInitialPortOpenAsync();

        try
        {
            await foreach (PortForwardAction action in portForwardChannel.Reader.ReadAllAsync(stoppingToken))
            {
                switch (action)
                {
                    case { Open: true, Port: var port } when !openedPorts.ContainsKey(port):
                        await OpenPortAsync(port, stoppingToken);
                        openedPorts.TryAdd(port, true);
                        break;
                    case { Open: false, Port: var port } when openedPorts.ContainsKey(port):
                        await ClosePortAsync(port, stoppingToken);
                        openedPorts.TryRemove(port, out bool _);
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            foreach (KeyValuePair<ushort, bool> pair in openedPorts)
            {
                await ClosePortAsync(pair.Key, CancellationToken.None);
                openedPorts.TryRemove(pair.Key, out bool _);
            }
            throw;
        }

        void ListenForOptionsChange()
        {
            IDisposable optionsMonitorDisposable = optionsProvider.OnChange(options =>
            {
                logger.LogTrace("Adjusting for options change...");
                if (options.AutoPortForward)
                {
                    // Remove ports opened prior. TODO: Delay closing port if player connections are still active?
                    foreach (KeyValuePair<ushort, bool> pair in openedPorts)
                    {
                        if (pair.Key == options.ServerPort)
                        {
                            continue;
                        }
                        portForwardChannel.Writer.TryWrite(new PortForwardAction(pair.Key, false));
                    }
                    // Open the new port.
                    portForwardChannel.Writer.TryWrite(new PortForwardAction(options.ServerPort, true));
                }
                else
                {
                    // Close all ports as port forwarding option is disabled.
                    foreach (KeyValuePair<ushort, bool> pair in openedPorts)
                    {
                        portForwardChannel.Writer.TryWrite(new PortForwardAction(pair.Key, false));
                    }
                }
            });
            stoppingToken.Register(() => optionsMonitorDisposable?.Dispose());
        }

        async Task QueueInitialPortOpenAsync()
        {
            if (optionsProvider.CurrentValue is { AutoPortForward: true, ServerPort: var port })
            {
                await portForwardChannel.Writer.WriteAsync(new PortForwardAction(port, true), stoppingToken);
            }
        }
    }

    private async Task OpenPortAsync(ushort port, CancellationToken cancellationToken = default)
    {
        if (await NatHelper.GetPortMappingAsync(port, Protocol.Udp, cancellationToken) != null)
        {
            logger.LogInformation("Port {Port} UDP is already port forwarded", port);
            return;
        }

        NatHelper.ResultCodes mappingResult = await NatHelper.AddPortMappingAsync(port, Protocol.Udp, cancellationToken);
        if (!cancellationToken.IsCancellationRequested)
        {
            switch (mappingResult)
            {
                case NatHelper.ResultCodes.SUCCESS:
                    logger.LogInformation("Server port {Port} UDP has been automatically opened on your router (port is closed when server closes)", port);
                    break;
                case NatHelper.ResultCodes.CONFLICT_IN_MAPPING_ENTRY:
                    logger.LogWarning("Port forward for {Port} UDP failed. It appears to already be port forwarded or it conflicts with another port forward rule.", port);
                    break;
                case NatHelper.ResultCodes.UNKNOWN_ERROR:
                    logger.LogWarning("Failed to port forward {Port} UDP through UPnP. If using Hamachi or you've manually port-forwarded, please disregard this warning. To disable this feature you can go into the server settings.", port);
                    break;
            }
        }
    }

    private async Task ClosePortAsync(ushort port, CancellationToken cancellationToken = default)
    {
        if (await NatHelper.DeletePortMappingAsync(port, Protocol.Udp, cancellationToken))
        {
            logger.LogInformation("Removed port forward rule {Port} UDP", port);
        }
        else
        {
            logger.LogInformation("Failed to remove port forward rule {Port} UDP", port);
        }
    }

    private record PortForwardAction(ushort Port, bool Open);
}
