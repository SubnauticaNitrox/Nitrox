using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors.Abstract
{
    public abstract class GenericPacketProcessor<T> : PacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet)
        {
            Process((T)packet);
        }

        public abstract void Process(T packet);
    }
}
