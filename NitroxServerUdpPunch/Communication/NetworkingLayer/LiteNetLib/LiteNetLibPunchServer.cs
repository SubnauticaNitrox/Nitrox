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
    class LiteNetLibPunchServer : INatPunchListener, IPunchServer
    {
        private readonly NetManager server;
        private readonly int port;
        private readonly EventBasedNetListener listener;
        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private readonly Dictionary<string, ServerIpInfo> tokenServerDict = new Dictionary<string, ServerIpInfo>();
        private readonly double timeoutInMinutes;
        

        private void ConsoleWriteLine(string text, params object[] param)
        {
            Console.WriteLine(DateTime.Now + ": " + text, param);
        }

        public LiteNetLibPunchServer(int port, double timeoutInMinutes)
        {
            listener = new EventBasedNetListener();
            server = new NetManager(listener);
            this.port = port;
            this.timeoutInMinutes = timeoutInMinutes;
        }

        public void Start()
        {
            
            listener.PeerConnectedEvent += peer =>
            {
                ConsoleWriteLine("PeerConnected: {0}. This should not happen!" , peer.EndPoint.ToString());
            };

            listener.ConnectionRequestEvent += request =>
            {
                ConsoleWriteLine("Get connection request from {0}", request.RemoteEndPoint);
                request.Reject();
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                ConsoleWriteLine("Peer {0} disconnected", peer.EndPoint);
            };

            listener.NetworkErrorEvent += (peer, error) =>
            {
                ConsoleWriteLine("Got error from {0} with code {1}", peer, error);
            };

            server.Start(port);
            server.DiscoveryEnabled = true;
            server.NatPunchEnabled = true;
            server.NatPunchModule.Init(this);
            
            ConsoleWriteLine("Server started on port {0}", port);
        }       

        public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            string[] tokenParse = token.Split("|");
            if(tokenParse.Count() > 2)
            {
                ConsoleWriteLine("Forbidden second | in token: {0}", token);
            }
            bool registerServer = tokenParse[0].ToLower() == "register"; 
            if (registerServer)
            {                
                string remoteIp = remoteEndPoint.Address.ToString();
                bool alreadyRegistered = tokenServerDict.ContainsKey(remoteIp);

                ServerIpInfo serverData;

                if(alreadyRegistered)
                {
                    serverData = tokenServerDict[remoteIp];
                    serverData.UpdateEndpoints(localEndPoint, remoteEndPoint);
                } else
                {
                    serverData = new ServerIpInfo(localEndPoint, remoteEndPoint);
                }

                // Put registered or updated in console output
                string registeredUpdated = alreadyRegistered ? "Updated" : "Registered";
                tokenServerDict[remoteIp] = serverData;
                ConsoleWriteLine("{2} server with internal {0} and external {1} address", localEndPoint, remoteEndPoint, registeredUpdated);
                // Register with game name
                if (tokenParse.Count() > 1 && tokenParse[1].Trim() != "")
                {
                    string serverName = tokenParse[1];
                    alreadyRegistered = tokenServerDict.ContainsKey(serverName);
                    if (alreadyRegistered && !tokenServerDict[serverName].RemoteEndPoint.Address.Equals(remoteEndPoint.Address))
                    {
                        NetDataWriter netData = new NetDataWriter();
                        string[] data = new string[2] { "Error", serverName };
                        netData.PutArray(data);
                        server.SendUnconnectedMessage(netData, remoteEndPoint);
                        ConsoleWriteLine("Got same servername {0} from server {2} for another server: {1}", serverName, remoteEndPoint.Address, tokenServerDict[serverName].RemoteEndPoint.Address);
                    }
                    else
                    {
                        tokenServerDict[serverName] = serverData;
                        // Put registered or Updated in string
                        registeredUpdated = alreadyRegistered ? "Updated" : "Registered";
                        ConsoleWriteLine("{0} server with game name {1}", registeredUpdated, serverName);
                    }
                }                                
            }
            else
            {
                ConsoleWriteLine("Try to introduce {0} with i({2}) e({1})", token, remoteEndPoint, localEndPoint);
                if (tokenServerDict.TryGetValue(token, out ServerIpInfo hostData) || tokenServerDict.TryGetValue(token, out hostData))
                {
                    server.NatPunchModule.NatIntroduce(
                    localEndPoint, // client internal
                    remoteEndPoint, // client external
                    hostData.LocalEndPoint, // host internal
                    hostData.RemoteEndPoint, // host external
                    token // request token
                    );
                    
                    ConsoleWriteLine("Introduced server {0} with client {1}", hostData.RemoteEndPoint, remoteEndPoint);
                }
            }
        }

        public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, string token)
        {
            ConsoleWriteLine("Success... Why?");
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
                if(tokenServer.Value.LastUpdated + TimeSpan.FromMinutes(timeoutInMinutes) < DateTime.Now)
                {
                    tokensToDelete.Add(tokenServer.Key);
                }
            }
            foreach(var token in tokensToDelete)
            {
                var serverData = tokenServerDict[token];
                ConsoleWriteLine("Timeout for server with internal {0} and external {1} address", serverData.LocalEndPoint, serverData.RemoteEndPoint);
                tokenServerDict.Remove(token);
            }
        }
    }
}
