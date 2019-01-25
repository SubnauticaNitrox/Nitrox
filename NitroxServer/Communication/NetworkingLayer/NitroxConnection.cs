using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxServer.Communication.NetworkingLayer
{
    public interface NitroxConnection : IProcessorContext
    {
        void SendPacket(Packet packet);
    }
}
