using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Constants;
using NitroxModel.Logger;

namespace NitroxClient.Communication
{
    public static class LANDiscoveryClient
    {
        private static readonly List<IPEndPoint> discoveredServers = new();

        private static Action<IPEndPoint> foundServerCallback;

        private static EventBasedNetListener listener;
        private static NetManager client;

        private static Timer broadcastTimer;
        private static Timer pollTimer;

        public static void SearchForServers(Action<IPEndPoint> callback)
        {
            listener?.ClearNetworkReceiveUnconnectedEvent();
            client?.Stop();
            broadcastTimer?.Dispose();
            pollTimer?.Dispose();

            foundServerCallback = callback;
            discoveredServers.Clear();

            Log.Info("Searching for LAN servers...");

            listener = new EventBasedNetListener();
            client = new NetManager(listener);

            client.AutoRecycle = true;
            client.DiscoveryEnabled = true;
            client.UnconnectedMessagesEnabled = true;

            client.Start(LANDiscoveryConstants.BROADCAST_PORT);

            listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnected;

            broadcastTimer = new Timer((state) =>
            {
                NetDataWriter writer = new();
                writer.Put(LANDiscoveryConstants.BROADCAST_REQUEST_STRING);

                client.SendDiscoveryRequest(writer, LANDiscoveryConstants.BROADCAST_PORT);
            });
            broadcastTimer.Change(0, 5000);

            pollTimer = new Timer((state) =>
            {
                client.PollEvents();
            });
            pollTimer.Change(0, 15);
        }

        private static void NetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                string responseString = reader.GetString();
                if (responseString == LANDiscoveryConstants.BROADCAST_RESPONSE_STRING)
                {
                    IPEndPoint serverEndPoint = reader.GetNetEndPoint();

                    if (!discoveredServers.Contains(serverEndPoint)) // prevents duplicate entries
                    {
                        Log.Info($"Found LAN server at {serverEndPoint}.");
                        discoveredServers.Add(serverEndPoint);
                        foundServerCallback(serverEndPoint);
                    }
                }
            }
        }
    }
}
