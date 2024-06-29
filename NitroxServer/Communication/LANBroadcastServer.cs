using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Constants;

namespace NitroxServer.Communication;

public static class LANBroadcastServer
{
    private static NetManager server;
    private static EventBasedNetListener listener;
    private static Timer pollTimer;

    public static void Start(CancellationToken ct)
    {
        listener = new EventBasedNetListener();
        listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnected;
        server = new NetManager(listener);
        server.AutoRecycle = true;
        server.BroadcastReceiveEnabled = true;
        server.UnconnectedMessagesEnabled = true;

        foreach (int port in LANDiscoveryConstants.BROADCAST_PORTS)
        {
            if (server.Start(port))
            {
                break;
            }
        }

        pollTimer = new Timer(_ => server.PollEvents());
        pollTimer.Change(0, 100);
        Log.Debug($"{nameof(LANBroadcastServer)} started");
    }

    public static void Stop()
    {
        listener?.ClearNetworkReceiveUnconnectedEvent();
        server?.Stop();
        pollTimer?.Dispose();
        Log.Debug($"{nameof(LANBroadcastServer)} stopped");
    }

    private static void NetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.Broadcast)
        {
            string requestString = reader.GetString();
            if (requestString == LANDiscoveryConstants.BROADCAST_REQUEST_STRING)
            {
                NetDataWriter writer = new();
                writer.Put(LANDiscoveryConstants.BROADCAST_RESPONSE_STRING);
                writer.Put(Server.Instance.Port);

                server.SendBroadcast(writer, remoteEndPoint.Port);
            }
        }
    }
}
