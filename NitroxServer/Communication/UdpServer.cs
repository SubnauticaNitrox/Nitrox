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

namespace NitroxServer.Communication
{
    public class UdpServer
    {
        private readonly PacketHandler packetHandler;
        private readonly PlayerManager playerManager;
        private readonly EntitySimulation entitySimulation;
        private readonly Dictionary<long, Connection> connectionsByRemoteIdentifier = new Dictionary<long, Connection>(); 
        private readonly NetServer server;
        static long downloadSpeed;

        public UdpServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation)
        {
            this.packetHandler = packetHandler;
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            NetPeerConfiguration config = BuildNetworkConfig();
            server = new NetServer(config);
        }

        public void Start()
        {
            server.Start();

            Thread thread = new Thread(Listen);
            thread.Start();
#if SWPF
            System.Timers.Timer Hora = new System.Timers.Timer();
            Hora.Elapsed += new System.Timers.ElapsedEventHandler(Hora_Elapsed);
            Hora.Interval = 1000;
            Hora.Enabled = true;
#endif
        }

#if SWPF
        static void Hora_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainWindow.Instance.NetworkSpeed(downloadSpeed, Connection.UploadSpeed);
            downloadSpeed = 0;
            Connection.UploadSpeed = 0;
        }
#endif

        

        private void Listen()
        {
            while (true)
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
#if SWPF
            downloadSpeed += data.Length;
#endif

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

                    List<SimulatedEntity> revokedGuids = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);

                    if (revokedGuids.Count > 0)
                    {
                        SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedGuids);
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
            config.Port = 11000;
            config.MaximumConnections = 100;
            config.AutoFlushSendQueue = true;

            return config;
        }
    }
}
