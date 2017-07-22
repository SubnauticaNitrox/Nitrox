using NitroxModel.Packets;

namespace NitroxServer.Communication.Packets.Processors.Abstract
{
    public abstract class GenericServerPacketProcessor<T> : ServerPacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet, Player player)
        {
            Process((T)packet, player);
        }

        public abstract void Process(T packet, Player player);
    }
}
