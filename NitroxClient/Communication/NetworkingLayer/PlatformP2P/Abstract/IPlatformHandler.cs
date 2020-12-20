using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxServer.Communication.NetworkingLayer.PlatformP2P.Abstract
{
    public interface IPlatformHandler
    {
        public bool IsInitialized();

        public bool Setup();

        public void SendPacket(Packet packet, RemotePlayer remotePlayer);

        public void Connect(RemotePlayer client);

        public void Disconnect(RemotePlayer client);
    }
}
