using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxServer.Communication.NetworkingLayer.PlatformP2P.Abstract
{
    public interface IPlatformHandler
    {
        public bool IsInitialized();

        public bool Setup();
    }
}
