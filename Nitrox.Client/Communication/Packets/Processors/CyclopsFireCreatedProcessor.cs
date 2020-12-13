using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class CyclopsFireCreatedProcessor : ClientPacketProcessor<CyclopsFireCreated>
    {
        private readonly IPacketSender packetSender;
        private readonly Fires fires;

        public CyclopsFireCreatedProcessor(IPacketSender packetSender, Fires fires)
        {
            this.packetSender = packetSender;
            this.fires = fires;
        }

        public override void Process(CyclopsFireCreated packet)
        {
            fires.Create(packet.FireCreatedData);
        }
    }
}
