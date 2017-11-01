using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors.Abstract
{
    public abstract class AuthenticatedPacketProcessor<T> : PacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet, IProcessorContext player)
        {
            Process((T)packet, (Player)player);
        }

        public abstract void Process(T packet, Player player);
    }
}
