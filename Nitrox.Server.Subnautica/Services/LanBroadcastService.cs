using System;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Constants;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Broadcasts the server port over LAN. Clients listening for this broadcast automatically add this server to the
///     server list.
/// </summary>
internal class LanBroadcastService(IOptionsMonitor<SubnauticaServerOptions> optionsProvider, ILogger<LanBroadcastService> logger) : BackgroundService
{
    private const int ACTIVE_POLL_INTERVAL_MS = 100;
    private const int INACTIVE_POLL_INTERVAL_MS = (int)(5 * TimeSpan.MillisecondsPerSecond);

    private readonly ILogger<LanBroadcastService> logger = logger;
    private readonly IOptionsMonitor<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly PeriodicTimer pollTimer = new(TimeSpan.FromMilliseconds(ACTIVE_POLL_INTERVAL_MS));
    private EventBasedNetListener listener;
    private int selectedPort;

    private NetManager server;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IDisposable optionsListenerDisposable = optionsProvider.OnChange(OptionsChanged);
        stoppingToken.Register(() => optionsListenerDisposable?.Dispose());
        listener = new EventBasedNetListener();
        listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnected;
        server = new NetManager(listener)
        {
            AutoRecycle = true,
            BroadcastReceiveEnabled = true,
            UnconnectedMessagesEnabled = true
        };

        UpdateServiceState(optionsProvider.CurrentValue);
        if (!server.IsRunning)
        {
            return;
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await pollTimer.WaitForNextTickAsync(stoppingToken);
                if (server.IsRunning)
                {
                    server.PollEvents();
                }
            }
        }
        catch (OperationCanceledException)
        {
            listener?.ClearNetworkReceiveUnconnectedEvent();
            server?.Stop();
            pollTimer?.Dispose();
            logger.ZLogDebug($"stopped");
            throw;
        }
    }

    private void OptionsChanged(SubnauticaServerOptions options) => UpdateServiceState(options, true);

    private void UpdateServiceState(SubnauticaServerOptions options, bool runAsUser = false)
    {
        if (runAsUser)
        {
            logger.ZLogDebug($"Adjusting to option changes...");
        }
        if (options.LanDiscoveryEnabled)
        {
            if (!StartListening())
            {
                return;
            }
            pollTimer.Period = TimeSpan.FromMilliseconds(ACTIVE_POLL_INTERVAL_MS);
            if (runAsUser)
            {
                logger.ZLogInformation($"enabled");
            }
            logger.LogDebug("broadcasting on port {Port}", selectedPort);
        }
        else
        {
            server.Stop();
            pollTimer.Period = TimeSpan.FromMilliseconds(INACTIVE_POLL_INTERVAL_MS);
            if (runAsUser)
            {
                logger.ZLogInformation($"disabled");
            }
        }
    }

    private bool StartListening()
    {
        if (selectedPort != 0)
        {
            if (server.Start(selectedPort))
            {
                return true;
            }
        }

        foreach (int port in LANDiscoveryConstants.BROADCAST_PORTS)
        {
            if (port == selectedPort)
            {
                continue;
            }
            if (server.Start(port))
            {
                selectedPort = port;
                break;
            }
        }
        if (selectedPort == 0)
        {
            logger.LogError("None of the broadcast ports are available");
            return false;
        }
        return true;
    }

    private void NetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType != UnconnectedMessageType.Broadcast)
        {
            return;
        }
        string requestString = reader.GetString();
        if (requestString != LANDiscoveryConstants.BROADCAST_REQUEST_STRING)
        {
            return;
        }

        ushort port = optionsProvider.CurrentValue.ServerPort;
        logger.LogDebug("Broadcasting server port {Port} over LAN...", port);
        NetDataWriter writer = new();
        writer.Put(LANDiscoveryConstants.BROADCAST_RESPONSE_STRING);
        writer.Put(port);
        server.SendBroadcast(writer, remoteEndPoint.Port);
    }
}
