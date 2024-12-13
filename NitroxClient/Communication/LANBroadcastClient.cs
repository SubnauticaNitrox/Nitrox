using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Constants;

namespace NitroxClient.Communication;

public static class LANBroadcastClient
{
    private static event Action<IPEndPoint> serverFound;

    public static event Action<IPEndPoint> ServerFound
    {
        add
        {
            serverFound += value;

            // Trigger event for servers already found.
            foreach (IPEndPoint server in DiscoveredServers)
            {
                value?.Invoke(server);
            }
        }
        remove => serverFound -= value;
    }

    private static Task<IEnumerable<IPEndPoint>> lastTask;
    public static ConcurrentQueue<IPEndPoint> DiscoveredServers = [];

    public static async Task<IEnumerable<IPEndPoint>> SearchAsync(bool force = false, CancellationToken cancellationToken = default)
    {
        if (!force && lastTask != null)
        {
            DiscoveredServers = [];
            foreach (IPEndPoint ipEndPoint in await lastTask)
            {
                DiscoveredServers.Enqueue(ipEndPoint);
            }
            return DiscoveredServers;
        }

        return await (lastTask = SearchInternalAsync(cancellationToken));
    }

    private static async Task<IEnumerable<IPEndPoint>> SearchInternalAsync(CancellationToken cancellationToken = default)
    {
        static void ReceivedResponse(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType != UnconnectedMessageType.Broadcast)
            {
                return;
            }
            string responseString = reader.GetString();
            if (responseString != LANDiscoveryConstants.BROADCAST_RESPONSE_STRING)
            {
                return;
            }
            int serverPort = reader.GetInt();
            IPEndPoint serverEndPoint = new(remoteEndPoint.Address, serverPort);
            if (DiscoveredServers.Contains(serverEndPoint))
            {
                return;
            }

            Log.Debug($"Found LAN server at {serverEndPoint}.");
            DiscoveredServers.Enqueue(serverEndPoint);
            OnServerFound(serverEndPoint);
        }

        cancellationToken = cancellationToken == default ? new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token : cancellationToken;
        EventBasedNetListener listener = new();
        NetManager client = new(listener)
        {
            AutoRecycle = true,
            BroadcastReceiveEnabled = true,
            UnconnectedMessagesEnabled = true
        };
        // Try start client on an available, predefined, port
        foreach (int port in LANDiscoveryConstants.BROADCAST_PORTS)
        {
            if (client.Start(port))
            {
                break;
            }
        }
        if (!client.IsRunning)
        {
            Log.Warn("Failed to start LAN discover client: none of the defined ports are available");
            return [];
        }

        Log.Info("Searching for LAN servers...");
        listener.NetworkReceiveUnconnectedEvent += ReceivedResponse;
        Task broadcastTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                NetDataWriter writer = new();
                writer.Put(LANDiscoveryConstants.BROADCAST_REQUEST_STRING);
                foreach (int port in LANDiscoveryConstants.BROADCAST_PORTS)
                {
                    client.SendBroadcast(writer, port);
                }
                try
                {
                    await Task.Delay(5000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            }
        }, cancellationToken);
        Task receiveTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                client.PollEvents();
                try
                {
                    await Task.Delay(100, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            }
        }, cancellationToken);

        await Task.WhenAll(broadcastTask, receiveTask);
        // Cleanup
        listener.ClearNetworkReceiveUnconnectedEvent();
        client.Stop();
        listener.NetworkReceiveUnconnectedEvent -= ReceivedResponse;
        return DiscoveredServers;
    }

    private static void OnServerFound(IPEndPoint obj)
    {
        serverFound?.Invoke(obj);
    }
}
