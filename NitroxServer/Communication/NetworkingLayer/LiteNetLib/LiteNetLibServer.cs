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
                Log.Debug("Got packet from upd server. Do nothing");
                //server.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("paschka.ddns.net", 11001), "register");
                //server.NatPunchModule.PollEvents();
            };

            NetManager netPeers = new NetManager(listener2);
            netPeers.UnsyncedEvents = true;
            netPeers.Start();
            netPeers.Connect("paschka.ddns.net", 11001,"NitroxPunch");

            server.NatPunchEnabled = true;
            server.NatPunchModule.Init(punchListener);            
            server.DiscoveryEnabled = true;
            server.UnconnectedMessagesEnabled = true;
            server.UpdateTime = 15;
            server.UnsyncedEvents = true;
            server.Start(portNumber);

            Thread backgroundPollThread = new Thread(() =>
            {
                DateTime time = DateTime.Now.Subtract(TimeSpan.FromMinutes(5));
                Log.Debug("Start nat punch poll thread");
                while (!isStopped)
                {
                    // Send punch register every minute
                    if (time + TimeSpan.FromMinutes(1) <= DateTime.Now)
                    {
                        Log.Debug("Send poll request");
                        time = DateTime.Now;
                        server.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("paschka.ddns.net", 11001), "register");
                    }
                    server.NatPunchModule.PollEvents();
                    Thread.Sleep(1000);
                }
            });
            backgroundPollThread.Start();
            Log.Debug("Register hole punch");
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
