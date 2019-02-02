using System;
using System.Collections.Generic;
using System.Threading;
using Lidgren.Network;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets;
using NitroxServer.ConfigParser;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.NetworkingLayer.Lidgren
{
    public class LidgrenServer : NitroxServer
    {
        private readonly NetServer netServer;
        private readonly Thread thread;

        public LidgrenServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig) : base(packetHandler, playerManager, entitySimulation, serverConfig)
        {
            NetPeerConfiguration config = BuildNetworkConfig();
            netServer = new NetServer(config);
            thread = new Thread(Listen);
        }

        public override void Start()
        {
            Log.Info("Using Lidgren as networking library");
            netServer.Start();
            thread.Start();

            isStopped = false;
        }

        public override void Stop()
        {
            isStopped = true;

            netServer.Shutdown("Shutting down server...");
            thread.Join(30000);
        }

       private void Listen()
        {
            while (!isStopped)
            {
                // Pause reading thread and wait for messages.
                netServer.MessageReceivedEvent.WaitOne();

                NetIncomingMessage im;
                while ((im = netServer.ReadMessage()) != null)
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
                                NitroxConnection connection = GetConnection(im.SenderConnection.RemoteUniqueIdentifier);
                                Packet packet = Packet.Deserialize(im.Data);
                                ProcessIncomingData(connection, packet);
                            }
                            break;
                        default:
                            Log.Info("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                            break;
                    }
                    netServer.Recycle(im);
                }
            }
        }

        private void ConnectionStatusChanged(NetConnectionStatus status, NetConnection networkConnection)
        {
            if (status == NetConnectionStatus.Connected)
            {
                LidgrenConnection connection = new LidgrenConnection(netServer, networkConnection);

                lock (connectionsByRemoteIdentifier)
                {
                    connectionsByRemoteIdentifier[networkConnection.RemoteUniqueIdentifier] = connection;
                }
            }
            else if (status == NetConnectionStatus.Disconnected)
            {
                ClientDisconnected(GetConnection(networkConnection.RemoteUniqueIdentifier));
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
