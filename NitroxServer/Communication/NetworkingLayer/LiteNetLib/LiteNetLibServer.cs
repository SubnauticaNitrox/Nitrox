using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.NetworkingLayer.LiteNetLib
{
    public class LiteNetLibServer : NitroxServer
    {
        private readonly NetManager server;
        private readonly EventBasedNetListener listener;
        private readonly NetPacketProcessor netPacketProcessor = new();

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
            server.DisconnectTimeout = 300000;  //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#endif
            if (!server.Start(portNumber))
            {
                return false;
            }
            
            SetupUPNP();
            
            isStopped = false;
            return true;
        }

        public override void Stop()
        {
            isStopped = true;
            server.Stop();
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

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (server.PeersCount < maxConn && !Banning.IpBanning.BannedIPs.Contains(request.RemoteEndPoint.Address.ToString()))
            {
                request.AcceptIfKey("nitrox");
            }
            else
            {
                request.Reject();
            }
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
