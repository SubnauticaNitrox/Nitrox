using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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
        private readonly EventBasedNatPunchListener punchListener;
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private bool serverNameTaken = false;
        private readonly IPEndPoint punchServerEndPoint;

        public LiteNetLibServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig) : base(packetHandler, playerManager, entitySimulation, serverConfig)
        {
            netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            punchListener = new EventBasedNatPunchListener();
            punchServerEndPoint = NetUtils.MakeEndPoint(serverConfig.UdpPunchServer, 11001);
        }
        public override void Start()
        {
            Log.Info("Using LiteNetLib as networking library");

            listener.PeerConnectedEvent += PeerConnected;
            listener.PeerDisconnectedEvent += PeerDisconnected;
            listener.NetworkReceiveEvent += NetworkDataReceived;
            listener.ConnectionRequestEvent += OnConnectionRequest;
            listener.NetworkReceiveUnconnectedEvent += OnUnconnectedUdpPacketRecieved;
            punchListener.NatIntroductionSuccess += (point, token) =>
            {                
                Log.Debug("Introduction success with {0}. This should not happen!", point);
            };            
            
            server.DiscoveryEnabled = true;
            server.UnconnectedMessagesEnabled = true;
            server.UpdateTime = 15;
            server.UnsyncedEvents = true;
            server.Start(portNumber);

            if (serverConfig.UdpPunchServer.Length != 0)
            {
                server.NatPunchEnabled = true;
                server.NatPunchModule.Init(punchListener);

                Thread backgroundPollThread = new Thread(() =>
                {
                    DateTime time = DateTime.Now.Subtract(TimeSpan.FromDays(5));
                    Log.Info("Start nat punch poll thread");
                    string token = "register";
                    if(serverConfig.ServerName != "")
                    {
                        token += "|";
                        token += serverConfig.ServerName;
                    }
                    
                    while (!isStopped)
                    {
                        // Send punch register every minute
                        if (time + TimeSpan.FromSeconds(serverConfig.UdpPunchRefreshTime) <= DateTime.Now)
                        {
                            Log.Info("Send poll request");
                            time = DateTime.Now;                            
                            server.NatPunchModule.SendNatIntroduceRequest(punchServerEndPoint, token);
                        }
                        server.NatPunchModule.PollEvents();
                        Thread.Sleep(100);
                    }
                });
                backgroundPollThread.Start();
            }

            isStopped = false;
        }

        public override void Stop()
        {
            isStopped = true;
            server.Stop();
        }

        private void PeerConnected(NetPeer peer)
        {
            Log.Debug("Peer connected: {0}", peer.EndPoint);
            LiteNetLibConnection connection = new LiteNetLibConnection(peer);

            lock (connectionsByRemoteIdentifier)
            {
                connectionsByRemoteIdentifier[peer.Id] = connection;
            }
        }

        private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var connection = GetConnection(peer.Id);
            if (connection != null)
            {
                ClientDisconnected(connection);
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
            ProcessIncomingData(connection, packet);
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

        public void OnUnconnectedUdpPacketRecieved(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (remoteEndPoint.Address.Equals(punchServerEndPoint))
            {
                string[] messages = reader.GetStringArray();
                if(messages.Length > 1)
                {
                    if (messages[0] == "Error")
                    {
                        if (messages[1] == serverConfig.ServerName && !serverNameTaken)
                        {
                            Log.Error("Server name already taken. Try again later or with another server name.");
                            serverNameTaken = true;
                        }
                    }
                }
            }
            else
            {
                Log.Warn("Got unconnected packet from unknown host {0} of type {1}.", remoteEndPoint, messageType);
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
