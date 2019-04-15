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

        public LiteNetLibServer(PacketHandler packetHandler, PlayerManager playerManager, EntitySimulation entitySimulation, ServerConfig serverConfig) : base(packetHandler, playerManager, entitySimulation, serverConfig)
        {
            netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            punchListener = new EventBasedNatPunchListener();
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
                Log.Debug("Introduction success with {0}", point);
                //server.Connect(point,"nitrox");
            };

            EventBasedNetListener listener2 = new EventBasedNetListener();
            listener2.ConnectionRequestEvent += (request) =>
            {
                Log.Debug("Accept punch connection request from {0}", new object[] { request.RemoteEndPoint });
                request.Accept();
            };

            listener2.PeerConnectedEvent += peer =>
            {
                Log.Debug("PeerConnected: " + peer.EndPoint.ToString());
            };

            listener2.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Log.Debug("Peer {0} disconnected", peer.EndPoint);
            };

            listener2.NetworkErrorEvent += (peer, error) =>
            {
                Log.Debug("Got error from {0} with code {1}", peer, error);
            };
            listener2.NetworkReceiveEvent += (peer, data, deliveryMethod) =>
            {
                Log.Debug("Got packet from upd server. Send packet to EndPoint");
                EndPoint endPoint = data.GetNetEndPoint();
                server.SendUnconnectedMessage(new byte[] { 0 }, (IPEndPoint)endPoint);
                //server.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("paschka.ddns.net", 11001), "register");
                //server.NatPunchModule.PollEvents();
            };

            NetManager netPeers = new NetManager(listener2);
            netPeers.UnsyncedEvents = true;
            //netPeers.Start();
            //netPeers.Connect("paschka.ddns.net", 11001,"NitroxPunch");
            
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
                    IPEndPoint punchAddress = NetUtils.MakeEndPoint(serverConfig.UdpPunchServer, 11001);
                    while (!isStopped)
                    {
                        // Send punch register every minute
                        if (time + TimeSpan.FromSeconds(serverConfig.UdpPunchRefreshTime) <= DateTime.Now)
                        {
                            Log.Info("Send poll request");
                            time = DateTime.Now;                            
                            server.NatPunchModule.SendNatIntroduceRequest(punchAddress, token);
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
            if (remoteEndPoint.Address.Equals(NetUtils.MakeEndPoint(serverConfig.UdpPunchServer, 11001)))
            {
                string[] messages = reader.GetStringArray();
                if(messages.Length > 1)
                {
                    if (messages[0] == "Error" && messages[1] == serverConfig.ServerName && !serverNameTaken)
                    {
                        Log.Error("Server name already taken. Try again later or with another server name.");
                        serverNameTaken = true;
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
