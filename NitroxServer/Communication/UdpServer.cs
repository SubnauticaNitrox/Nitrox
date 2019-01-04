using System;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.GameLogic;
using Lidgren.Network;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using NitroxServer.GameLogic.Entities;
using NitroxModel.DataStructures;
using NitroxServer.ConfigParser;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Core;
using NitroxServer.Communication.Packets.Processors;

namespace NitroxServer.Communication
{
    public class UdpServer
    {
        private bool isStopped = true;

        private readonly PacketHandler packetHandler;
        private readonly EntitySimulation entitySimulation;
        private readonly Dictionary<long, Connection> connectionsByRemoteIdentifier = new Dictionary<long, Connection>();
        private readonly PlayerManager playerManager;
        private readonly NetServer server;
        private readonly Thread thread;
        private int portNumber, maxConn;

        public UdpServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig)
        {
            this.packetHandler = packetHandler;
            this.entitySimulation = entitySimulation;
            this.playerManager = playerManager;

            portNumber = serverConfig.ServerPort;
            maxConn = serverConfig.MaxConnections;

            NetPeerConfiguration config = BuildNetworkConfig();
            server = new NetServer(config);
            thread = new Thread(Listen);
        }

        public void Start()
        {
            server.Start();
            thread.Start();
            
            
            isStopped = false;
        }

        public void Stop()
        {
            isStopped = true;

            server.Shutdown("Shutting down server...");
            thread.Join(30000);
        }
        
        private void Listen()
        {
            while (!isStopped)
            {
                // Pause reading thread and wait for messages.
                server.MessageReceivedEvent.WaitOne();

                NetIncomingMessage im;
                while ((im = server.ReadMessage()) != null)
                {
                    switch (im.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.ErrorMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                            string message = im.ReadString();
                            Log.Info("Networking: " + message);
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                            string reason = im.ReadString();
                            Log.Info("Identifier " + im.SenderConnection.RemoteUniqueIdentifier + " " + status + ": " + reason);

                            ConnectionStatusChanged(status, im.SenderConnection);
                            break;
                        case NetIncomingMessageType.Data:
                            if (im.Data.Length > 0)
                            {
                                Connection connection = GetConnection(im.SenderConnection.RemoteUniqueIdentifier);
                                ProcessIncomingData(connection, im.Data);
                            }
                            break;
                        default:
                            Log.Info("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                            break;
                    }
                    server.Recycle(im);
                }
            }
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

        private void ConnectionStatusChanged(NetConnectionStatus status, NetConnection networkConnection)
        {
            if (status == NetConnectionStatus.Connected)
            {
                Connection connection = new Connection(server, networkConnection);

                lock (connectionsByRemoteIdentifier)
                {
                    connectionsByRemoteIdentifier[networkConnection.RemoteUniqueIdentifier] = connection;
                }
            }
            else if (status == NetConnectionStatus.Disconnected)
            {
                Connection connection = GetConnection(networkConnection.RemoteUniqueIdentifier);
                Player player = playerManager.GetPlayer(connection);

                if (player != null)
                {
                    playerManager.PlayerDisconnected(connection);

                    Disconnect disconnect = new Disconnect(player.Id);
                    playerManager.SendPacketToAllPlayers(disconnect);

                    List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);

                    if (ownershipChanges.Count > 0)
                    {
                        SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(ownershipChanges);
                        playerManager.SendPacketToAllPlayers(ownershipChange);
                    }
                }
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

        private NetPeerConfiguration BuildNetworkConfig()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("Nitrox");
            config.Port = portNumber;
            config.MaximumConnections = maxConn;
            config.AutoFlushSendQueue = true;

            return config;
        }
    }
}
