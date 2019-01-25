using System;
using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.ConfigParser;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.NetworkingLayer.LiteNetLib
{
    public class LiteNetLibServer : NitroxServer
    {
        private const string CONNECTION_KEY = "nitrox";
        private readonly NetManager server;
        private readonly EventBasedNetListener listener;
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();

        public LiteNetLibServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig) : base(packetHandler, playerManager, entitySimulation, serverConfig)
        {
            netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
        }
        public override void Start()
        {
            Log.Info("Using LiteNetLib as networking library");

            listener.PeerConnectedEvent += PeerConnected;
            listener.PeerDisconnectedEvent += PeerDisconnected;
            listener.NetworkReceiveEvent += NetworkDataReceived;
            listener.ConnectionRequestEvent += OnConnectionRequest;

            server.DiscoveryEnabled = true;
            server.UnconnectedMessagesEnabled = true;
            server.UpdateTime = 15;
            server.UnsyncedEvents = true;
            server.Start(portNumber);

            isStopped = false;
        }

        public override void Stop()
        {
            isStopped = true;
            server.Stop();
        }

        private void PeerConnected(NetPeer peer)
        {
            LiteNetLibConnection connection = new LiteNetLibConnection(peer);

            lock (connectionsByRemoteIdentifier)
            {
                connectionsByRemoteIdentifier[peer.Id] = connection;
            }
        }

        private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            NitroxConnection connection = GetConnection(peer.Id);
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
            NitroxConnection connection = GetConnection(peer.Id);
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

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (server.PeersCount < maxConn)
            {
                request.AcceptIfKey("nitrox");
            }
            else
            {
                request.Reject();
            }
        }

        private void ProcessIncomingData(LiteNetLibConnection connection, byte[] data)
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

        private NitroxConnection GetConnection(long remoteIdentifier)
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
