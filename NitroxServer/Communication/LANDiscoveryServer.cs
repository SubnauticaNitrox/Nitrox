using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Constants;

namespace NitroxServer.Communication
{
    public static class LANDiscoveryServer
    {
        private static NetManager server;
        private static EventBasedNetListener listener;

        private static Timer pollTimer;

        public static void Start()
        {
            listener = new EventBasedNetListener();
            server = new NetManager(listener);

            server.AutoRecycle = true;
            server.DiscoveryEnabled = true;
            server.UnconnectedMessagesEnabled = true;

            for (int i = 0; i < LANDiscoveryConstants.BROADCAST_PORTS.Length; i++)
            {
                if (server.Start(LANDiscoveryConstants.BROADCAST_PORTS[i]))
                {
                    break;
                }
            }

            listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnected;

            pollTimer = new Timer((state) =>
            {
                server.PollEvents();
            });
            pollTimer.Change(0, 100);
        }

        private static void NetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.DiscoveryRequest)
            {
                string requestString = reader.GetString();
                if (requestString == LANDiscoveryConstants.BROADCAST_REQUEST_STRING)
                {
                    NetDataWriter writer = new();
                    writer.Put(LANDiscoveryConstants.BROADCAST_RESPONSE_STRING);
                    writer.Put(Server.Instance.Port);

                    server.SendDiscoveryResponse(writer, remoteEndPoint);
                }
            }
        }

        public static void Stop()
        {
            listener?.ClearNetworkReceiveUnconnectedEvent();
            server?.Stop();
            pollTimer?.Dispose();
        }
    }
}
