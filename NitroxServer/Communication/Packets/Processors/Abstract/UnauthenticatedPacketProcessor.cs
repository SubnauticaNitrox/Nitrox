using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors.Abstract
{
    public abstract class UnauthenticatedPacketProcessor<T> : PacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet, IProcessorContext connection)
        {
            Process((T)packet, (NitroxConnection)connection);
        }

        public abstract void Process(T packet, NitroxConnection connection);
    }
}
