using Nitrox.Model.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Models.Communication;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core
{
    public abstract class UnauthenticatedPacketProcessor<T> : PacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet, IProcessorContext connection)
        {
            Process((T)packet, (INitroxConnection)connection);
        }

        public abstract void Process(T packet, INitroxConnection connection);
    }
}
