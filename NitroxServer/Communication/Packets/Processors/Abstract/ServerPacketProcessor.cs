using NitroxModel.Packets;

namespace NitroxServer.Communication.Packets.Processors.Abstract
{
    public abstract class ServerPacketProcessor
    {
        public abstract void ProcessPacket(Packet packet, Player player);
    }
}
