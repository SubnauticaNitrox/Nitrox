using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxClient.Communication.Packets.Processors.Abstract
{
    public abstract class ClientPacketProcessor<T> : PacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet, IProcessorContext context)
        {
            Process((T)packet);
        }

        public abstract void Process(T packet);
    }
}
