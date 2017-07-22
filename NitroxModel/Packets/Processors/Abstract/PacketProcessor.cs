using NitroxModel.Packets;

namespace NitroxModel.Packets.Processors.Abstract
{
    public abstract class PacketProcessor
    {
        public abstract void ProcessPacket(Packet packet, IProcessorContext context);
    }
}
