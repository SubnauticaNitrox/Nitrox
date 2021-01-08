using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.NetworkingLayer.PlatformP2P.Abstract;
using Steamworks;

namespace NitroxClient.Communication.NetworkingLayer.PlatformP2P.Steam
{
    public class SteamP2PClient : IPlatformHandler, IClient
    {
        public bool IsConnected { get; private set; }

        private IClient host;

        public SteamP2PClient()
        {

        }

        public bool IsInitialized()
        {
            throw new NotImplementedException();
        }

        public void Send(Packet packet)
        {
            throw new NotImplementedException();
        }

        public void SendPacket(Packet packet, RemotePlayer remotePlayer)
        {
            throw new NotImplementedException();
        }

        public bool Setup()
        {
            throw new NotImplementedException();
        }

        public void Start(IConnectionInfo connectionInfo)
        {
            SteamP2P steamP2P = connectionInfo as SteamP2P;
            if (steamP2P == null)
            {
                throw new NotSupportedException("Tried passing incorrect connectionInfo to clientConnection");
            }

            
        }

        public void Stop()
        {

        }
    }
}
