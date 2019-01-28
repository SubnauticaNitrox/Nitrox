using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FireCreatedProcessor : ClientPacketProcessor<FireCreated>
    {
        private readonly IPacketSender packetSender;
        private readonly Fires fires;

        public FireCreatedProcessor(IPacketSender packetSender, Fires fires)
        {
            this.packetSender = packetSender;
            this.fires = fires;
        }

        public override void Process(FireCreated packet)
        {
            fires.Create(packet.FireCreatedData.FireGuid, packet.FireCreatedData.CyclopsGuid, packet.FireCreatedData.Room, packet.FireCreatedData.NodeIndex);
        }
    }
}
