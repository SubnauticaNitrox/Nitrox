using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Mono.Nat;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.LiteNetLib
{
    public class LiteNetLibServer : NitroxServer
    {
        private readonly EventBasedNetListener listener;
        private readonly NetPacketProcessor netPacketProcessor = new();
        private readonly NetManager server;

        public LiteNetLibServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig) : base(packetHandler, playerManager, entitySimulation, serverConfig)
        {
            netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
        }

        public override bool Start()
        {
            listener.PeerConnectedEvent += PeerConnected;
            listener.PeerDisconnectedEvent += PeerDisconnected;
            listener.NetworkReceiveEvent += NetworkDataReceived;
            listener.ConnectionRequestEvent += OnConnectionRequest;

            server.DiscoveryEnabled = true;
            server.UnconnectedMessagesEnabled = true;
            server.UpdateTime = 15;
            server.UnsyncedEvents = true;
#if DEBUG
            server.DisconnectTimeout = 300000; //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#endif

            if (!server.Start(portNumber))
            {
                return false;
            }
            
            if (useUpnpPortForwarding)
            {
                PortForwardAsync((ushort)portNumber).ConfigureAwait(false);
            }

            if (useLANDiscovery)
            {
                LANDiscoveryServer.Start();
            }

            return true;
        }

        private async Task PortForwardAsync(ushort port)
        {
            if (await NatHelper.GetPortMappingAsync(port, Protocol.Udp) != null)
            {
                Log.Info($"Port {port} UDP is already port forwarded");
                return;
            }

            bool isMapped = await NatHelper.AddPortMappingAsync(port, Protocol.Udp);
            if (isMapped)
            {
                Log.Info($"Server port {port} UDP has been automatically opened on your router (port is closed when server closes)");
            }
            else
            {
                Log.Warn($"Failed to automatically port forward {port} UDP through UPnP. If using Hamachi or manually port-forwarding, please disregard this warning. To disable this feature you can go into the server settings.");
            }
        }

        public override void Stop()
        {
            playerManager.SendPacketToAllPlayers(new ServerStopped());
            // We want every player to receive this packet
            Thread.Sleep(500);
            server.Stop();
            if (useUpnpPortForwarding)
            {
                NatHelper.DeletePortMappingAsync((ushort)portNumber, Protocol.Udp).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (useLANDiscovery)
            {
                LANDiscoveryServer.Stop();
            }
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (server.PeersCount < maxConnections)
            {
                request.AcceptIfKey("nitrox");
            }
            else
            {
                request.Reject();
            }
        }

        private void PeerConnected(NetPeer peer)
        {
            LiteNetLibConnection connection = new(peer);
            lock (connectionsByRemoteIdentifier)
            {
                connectionsByRemoteIdentifier[peer.Id] = connection;
            }
        }

        private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            ClientDisconnected(GetConnection(peer.Id));
        }

        private void NetworkDataReceived(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void OnPacketReceived(WrapperPacket wrapperPacket, NetPeer peer)
        {
            NitroxConnection connection = GetConnection(peer.Id);
            Packet packet = Packet.Deserialize(wrapperPacket.packetData);
            ProcessIncomingData(connection, packet);
        }

        private NitroxConnection GetConnection(int remoteIdentifier)
        {
            NitroxConnection connection;
            lock (connectionsByRemoteIdentifier)
            {
                connectionsByRemoteIdentifier.TryGetValue(remoteIdentifier, out connection);
            }
            return connection;
        }
    }
}
