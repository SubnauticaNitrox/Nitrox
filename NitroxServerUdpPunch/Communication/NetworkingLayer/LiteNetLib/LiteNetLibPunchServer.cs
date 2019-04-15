using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NitroxServerUdpPunch.Communication.NetworkingLayer.LiteNetLib
{
    class LiteNetLibPunchServer : INatPunchListener
    {
        private readonly NetManager server;
        private readonly int port;
        private readonly EventBasedNetListener listener;
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private readonly Dictionary<string, Tuple<IPEndPoint, IPEndPoint, DateTime>> tokenServerDict = new Dictionary<string, Tuple<IPEndPoint, IPEndPoint, DateTime>>();
        private double timeoutTimeInMinutes = 3;

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
            string[] tokenParse = token.Split("|");
            if(tokenParse.Count() > 2)
            {
                Console.WriteLine("Forbidden second | in token: {0}", token);
            }
            bool registerServer = tokenParse[0].ToLower() == "register"; 
            if (registerServer)
            {
                var serverData = new Tuple<IPEndPoint, IPEndPoint, DateTime>(localEndPoint, remoteEndPoint, DateTime.Now);

                string remoteIp = remoteEndPoint.Address.ToString();
                bool alreadyRegistered = tokenServerDict.ContainsKey(remoteIp);
                // Put registered or updated in console output
                string registeredUpdated = alreadyRegistered ? "Updated" : "Registered";
                tokenServerDict[remoteIp] = serverData;
                Console.WriteLine("{2} server with internal {0} and external {1} address", localEndPoint, remoteEndPoint, registeredUpdated);
                // Register with game name
                if (tokenParse.Count() > 1 && tokenParse[1].Trim() != "")
                {
                    string serverName = tokenParse[1];
                    alreadyRegistered = tokenServerDict.ContainsKey(serverName);
                    if (alreadyRegistered && !tokenServerDict[serverName].Item2.Address.Equals(remoteEndPoint.Address))
                    {
                        NetDataWriter netData = new NetDataWriter();
                        string[] data = new string[2] { "Error", serverName };
                        netData.PutArray(data);
                        server.SendUnconnectedMessage(netData, remoteEndPoint);
                        Console.WriteLine("Got same servername {0} from server {2} for another server: {1}", serverName, remoteEndPoint.Address, tokenServerDict[serverName].Item2.Address);
                    }
                    else
                    {
                        tokenServerDict[serverName] = serverData;
                        // Put registered or Updated in string
                        registeredUpdated = alreadyRegistered ? "Updated" : "Registered";
                        Console.WriteLine("{0} server with game name {1}", registeredUpdated, serverName);
                    }
                }                                
            }
            else
            {
                Tuple<IPEndPoint, IPEndPoint, DateTime> hostData;
                Console.WriteLine("Try to introduce {0} with e({1}) i({2})", token, remoteEndPoint, localEndPoint);
                if(tokenServerDict.TryGetValue(token,out hostData) || tokenServerDict.TryGetValue(token, out hostData))
                {
                    server.NatPunchModule.NatIntroduce(
                    localEndPoint, // client internal
                    remoteEndPoint, // client external
                    hostData.Item1, // host internal
                    hostData.Item2, // host external
                    token // request token
                    );
                    var peers = (from p in server.ConnectedPeerList
                                 where p.EndPoint.Address.ToString() == token
                                 select p);
                    if (peers.Count() > 0)
                    {
                        var peer = peers.First();
                        NetDataWriter netDataWriter = new NetDataWriter();
                        netDataWriter.Put(remoteEndPoint);
                        peer.Send(netDataWriter, DeliveryMethod.Unreliable);
                    }
                    peers = (from p in server.ConnectedPeerList
                                 where p.EndPoint.Address.ToString() == remoteEndPoint.Address.ToString()
                             select p);
                    if(peers.Count() > 0)
                    {
                        var peer = peers.First();
                        var netDataWriter = new NetDataWriter();
                        netDataWriter.Put(hostData.Item2);
                        peer.Send(netDataWriter, DeliveryMethod.Unreliable);
                    }
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
            CheckTimeOuts();
        }

        private void CheckTimeOuts()
        {
            List<string> tokensToDelete = new List<string>();
            foreach(var tokenServer in tokenServerDict)
            {
                if(tokenServer.Value.Item3 + TimeSpan.FromMinutes(timeoutTimeInMinutes) < DateTime.Now)
                {
                    tokensToDelete.Add(tokenServer.Key);
                }
            }
            foreach(var token in tokensToDelete)
            {
                var serverData = tokenServerDict[token];
                Console.WriteLine("Timeout for server with internal {0} and external {1} address", serverData.Item1, serverData.Item2);
                tokenServerDict.Remove(token);
            }
        }
    }
}
