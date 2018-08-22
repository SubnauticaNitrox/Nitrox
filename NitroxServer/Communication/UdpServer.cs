using System;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using System.Collections.Generic;
using NitroxServer.GameLogic.Entities;
using NitroxModel.DataStructures;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Threading;

namespace NitroxServer.Communication
{
    public class UdpServer
    {
        private const int MAX_CLIENTS = ushort.MaxValue;
        private const string CONNECTION_KEY = "nitrox";
        private const int SERVER_PORT = 11000;

        private readonly PacketHandler packetHandler;
        private readonly PlayerManager playerManager;
        private readonly EntitySimulation entitySimulation;
        private readonly Dictionary<long, Connection> connectionsByRemoteIdentifier = new Dictionary<long, Connection>(); 
        private readonly NetManager server;
        private readonly EventBasedNetListener listener;

        public UdpServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation)
        {
            this.packetHandler = packetHandler;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;
            this.listener = new EventBasedNetListener();
            this.server = new NetManager(listener, MAX_CLIENTS, CONNECTION_KEY);
        }

        public void Start()
        {
            listener.PeerConnectedEvent += PeerConnected;
            listener.PeerDisconnectedEvent += PeerDisconnected;
            listener.NetworkReceiveEvent += NetworkDataReceived;
            listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;

            server.DiscoveryEnabled = true;
            server.UnconnectedMessagesEnabled = true;
            server.UpdateTime = 15;
            server.UnsyncedEvents = true;
            server.Start(SERVER_PORT);
        }

        private void PeerConnected(NetPeer peer)
        {
            Connection connection = new Connection(peer);

            lock (connectionsByRemoteIdentifier)
            {
                connectionsByRemoteIdentifier[peer.ConnectId] = connection;
            }
        }

        private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Connection connection = GetConnection(peer.ConnectId);
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

        private void NetworkDataReceived(NetPeer peer, NetDataReader reader)
        {
            Connection connection = GetConnection(peer.ConnectId);
            Packet packet = Packet.Deserialize(reader.Data);

            try
            {
                packetHandler.Process(packet, connection);
            }
            catch (Exception ex)
            {
                Log.Info("Exception while processing packet: " + packet + " " + ex);
            }
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            int i = 0;
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
