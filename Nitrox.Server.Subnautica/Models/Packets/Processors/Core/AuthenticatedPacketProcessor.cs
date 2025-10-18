using Nitrox.Model.Packets.Processors.Abstract;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core
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
