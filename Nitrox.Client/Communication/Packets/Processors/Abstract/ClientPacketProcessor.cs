using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Processors.Abstract;

namespace Nitrox.Client.Communication.Packets.Processors.Abstract
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
