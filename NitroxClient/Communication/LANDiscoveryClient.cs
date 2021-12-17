using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Constants;

namespace NitroxClient.Communication
{
    public static class LANDiscoveryClient
    {
        private static readonly List<IPEndPoint> discoveredServers = new();

        private static EventBasedNetListener listener;
        private static NetManager client;

        private static Timer broadcastTimer;
        private static Timer pollTimer;

        public static event Action<IPEndPoint> ServerFound;

        public static void BeginSearching()
        {
            EndSearching();

            Log.Info("Searching for LAN servers...");

            listener = new EventBasedNetListener();
            client = new NetManager(listener);

            client.AutoRecycle = true;
            client.DiscoveryEnabled = true;
            client.UnconnectedMessagesEnabled = true;

            foreach (int port in LANDiscoveryConstants.BROADCAST_PORTS)
            {
                if (client.Start(port))
                {
                    break;
                }
            }

            listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnected;

            broadcastTimer = new Timer((state) =>
            {
                NetDataWriter writer = new();
                writer.Put(LANDiscoveryConstants.BROADCAST_REQUEST_STRING);

                client.SendDiscoveryRequest(writer, client.LocalPort);
            });
            broadcastTimer.Change(0, 5000);

            pollTimer = new Timer((state) =>
            {
                client.PollEvents();
            });
            pollTimer.Change(0, 100);
        }

        public static void EndSearching()
        {
            listener?.ClearNetworkReceiveUnconnectedEvent();
            client?.Stop();
            broadcastTimer?.Dispose();
            pollTimer?.Dispose();

            listener = null;
            client = null;
            broadcastTimer = null;
            pollTimer = null;

            discoveredServers.Clear();
        }

        private static void NetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                string responseString = reader.GetString();
                if (responseString == LANDiscoveryConstants.BROADCAST_RESPONSE_STRING)
                {
                    int serverPort = reader.GetInt();
                    IPEndPoint serverEndPoint = new(remoteEndPoint.Address, serverPort);

                    if (!discoveredServers.Contains(serverEndPoint)) // prevents duplicate entries
                    {
                        Log.Info($"Found LAN server at {serverEndPoint}.");
                        discoveredServers.Add(serverEndPoint);
                        ServerFound(serverEndPoint);
                    }
                }
            }
        }
    }
}
