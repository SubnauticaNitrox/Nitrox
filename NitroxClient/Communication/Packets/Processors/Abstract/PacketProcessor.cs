using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors.Abstract
{
    public abstract class PacketProcessor
    {
        public abstract void ProcessPacket(Packet packet);
    }
}
