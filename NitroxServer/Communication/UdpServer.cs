using System;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxServer.GameLogic.Entities;
using NitroxModel.DataStructures;
using NitroxServer.ConfigParser;

namespace NitroxServer.Communication
{
    public class UdpServer
    {
        private bool isStopped = true;

        private const int MAX_CLIENTS = ushort.MaxValue;
        private const string CONNECTION_KEY = "nitrox";
        private const int SERVER_PORT = 11000;

        private readonly PacketHandler packetHandler;
        private readonly EntitySimulation entitySimulation;
        private readonly Dictionary<long, Connection> connectionsByRemoteIdentifier = new Dictionary<long, Connection>();
        private readonly PlayerManager playerManager;
        private readonly NetManager server;
        private readonly EventBasedNetListener listener;
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private int portNumber, maxConn;

        public UdpServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig)
        {
            this.packetHandler = packetHandler;
            this.entitySimulation = entitySimulation;
            this.playerManager = playerManager;

            portNumber = serverConfig.ServerPort;
            maxConn = serverConfig.MaxConnections;

            netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);

            listener = new EventBasedNetListener();
            server = new NetManager(listener);
        }

        public void Start()
        {
            listener.PeerConnectedEvent += PeerConnected;
            listener.PeerDisconnectedEvent += PeerDisconnected;
            listener.NetworkReceiveEvent += NetworkDataReceived;
            listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;
            listener.ConnectionRequestEvent += OnConnectionRequest;

            server.DiscoveryEnabled = true;
            server.UnconnectedMessagesEnabled = true;
            server.UpdateTime = 15;
            server.UnsyncedEvents = true;
            server.Start(SERVER_PORT);

            isStopped = false;
        }

        private void PeerConnected(NetPeer peer)
        {
            Connection connection = new Connection(peer);

            lock (connectionsByRemoteIdentifier)
            {
                connectionsByRemoteIdentifier[peer.Id] = connection;
            }
        }

        private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Connection connection = GetConnection(peer.Id);
            Player player = playerManager.GetPlayer(connection);

            if (player != null)
            {
                playerManager.PlayerDisconnected(connection);

                Disconnect disconnect = new Disconnect(player.Id);
                playerManager.SendPacketToAllPlayers(disconnect);

                List<SimulatedEntity> revokedGuids = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);

                if (revokedGuids.Count > 0)
                {
                    SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedGuids);
                    playerManager.SendPacketToAllPlayers(ownershipChange);
                }
            }
        }

        private void NetworkDataReceived(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void OnPacketReceived(WrapperPacket wrapperPacket, NetPeer peer)
        {
            Connection connection = GetConnection(peer.Id);
            Packet packet = Packet.Deserialize(wrapperPacket.packetData);

            try
            {
                packetHandler.Process(packet, connection);
            }
            catch (Exception ex)
            {
                Log.Info("Exception while processing packet: " + packet + " " + ex);
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            int i = 0;
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (server.PeersCount < MAX_CLIENTS)
            {
                request.AcceptIfKey("nitrox");
            }
            else
            {
                request.Reject();
            }
        }

        public void Stop()
        {
            isStopped = true;

            server.Stop();
        }

        private void ProcessIncomingData(Connection connection, byte[] data)
        {
            Packet packet = Packet.Deserialize(data);

            try
            {
                packetHandler.Process(packet, connection);
            }
            catch (Exception ex)
            {
                Log.Info("Exception while processing packet: " + packet + " " + ex);
            }
        }

        private Connection GetConnection(long remoteIdentifier)
        {
            Connection connection;

            lock (connectionsByRemoteIdentifier)
            {
                connectionsByRemoteIdentifier.TryGetValue(remoteIdentifier, out connection);
            }

            return connection;
        }
    }
}
