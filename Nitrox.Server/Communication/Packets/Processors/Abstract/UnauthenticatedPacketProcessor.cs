using Nitrox.Model.Packets;
using Nitrox.Model.Packets.Processors.Abstract;
using Nitrox.Server.Communication.NetworkingLayer;

namespace Nitrox.Server.Communication.Packets.Processors.Abstract
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
