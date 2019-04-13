using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NitroxUdpPunchServer.Communication.NetworkingLayer.LiteNetLib
{
    struct Endpoint
    {

    }

    class LiteNetLibPunchServer : INatPunchListener
    {
        private const string CONNECTION_KEY = "nitrox";
        private readonly NetManager server;
        private readonly int port;
        private readonly EventBasedNetListener listener;
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private readonly Dictionary<string, Tuple<IPEndPoint, IPEndPoint, DateTime>> tokenServerDict = new Dictionary<string, Tuple<IPEndPoint, IPEndPoint, DateTime>>();

        public string ConnectionKey { get; private set; }

        public LiteNetLibPunchServer(int port, string connectionKey)
        {
            //netPacketProcessor.SubscribeReusable<string, NetPeer>(OnPacketReceived);
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            this.port = port;
            ConnectionKey = connectionKey;
        }

        public void Start()
        {
            
            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("PeerConnected: " + peer.EndPoint.ToString());
            };

            listener.ConnectionRequestEvent += request =>
            {
                Console.WriteLine("Get connection request from {0}", request.RemoteEndPoint);
                request.AcceptIfKey(ConnectionKey);
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Console.WriteLine("Peer {0} disconnected", peer.EndPoint);
                var server = tokenServerDict[peer.EndPoint.Address.ToString()];
                tokenServerDict.Remove(server.Item1.Address.ToString());
                tokenServerDict.Remove(server.Item2.Address.ToString());
            };

            listener.NetworkErrorEvent += (peer, error) =>
            {
                Console.WriteLine("Got error from {0} with code {1}", peer, error);
            };

            server.Start(port);
            server.DiscoveryEnabled = true;
            server.NatPunchEnabled = true;
            server.NatPunchModule.Init(this);
            
            Console.WriteLine("Server started on port {0}", port);
        }       

        public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            if (token == "register")
            {
                string localIp = localEndPoint.Address.ToString();
                string remoteIp = remoteEndPoint.Address.ToString();
                tokenServerDict[localIp] = new Tuple<IPEndPoint, IPEndPoint, DateTime>(localEndPoint, remoteEndPoint, DateTime.Now);
                tokenServerDict[remoteIp] = new Tuple<IPEndPoint, IPEndPoint, DateTime>(localEndPoint, remoteEndPoint, DateTime.Now);
                Console.WriteLine("Registered new server with internal {0} and external {1} address", localEndPoint, remoteEndPoint);
            }
            else
            {
                Tuple<IPEndPoint, IPEndPoint, DateTime> hostData;
                Console.WriteLine("Try to introduce {0} with e({1}) i({2})", token, remoteEndPoint, localEndPoint);
                if(tokenServerDict.TryGetValue(token,out hostData) || tokenServerDict.TryGetValue(token, out hostData))
                {
                    server.NatPunchModule.NatIntroduce(
                    hostData.Item1, // host internal
                    hostData.Item2, // host external
                    localEndPoint, // client internal
                    remoteEndPoint, // client external
                    token // request token
                    );
                    var peer = (from p in server.ConnectedPeerList
                                where p.EndPoint.Address.ToString() == token
                                select p).First();
                    NetSerializer serializer = new NetSerializer();
                    NetDataWriter writer = new NetDataWriter();
                    writer.Put(hostData.Item2);
                    peer.Send(writer.Data, DeliveryMethod.ReliableUnordered);
                    Console.WriteLine("Introduced server {0} with client {1}", hostData.Item2, remoteEndPoint);
                }
            }
        }

        public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, string token)
        {
            Console.WriteLine("Success... Why?");
            // Not needed
        }

        public void Process()
        {
            server.PollEvents();
            server.NatPunchModule.PollEvents();
        }
        
    }
}
