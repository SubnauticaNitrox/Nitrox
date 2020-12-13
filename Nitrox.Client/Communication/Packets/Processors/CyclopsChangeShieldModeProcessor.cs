using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class CyclopsChangeShieldModeProcessor : ClientPacketProcessor<CyclopsChangeShieldMode>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsChangeShieldModeProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsChangeShieldMode shieldPacket)
        {
            cyclops.ChangeShieldMode(shieldPacket.Id, shieldPacket.IsOn);
        }
    }
}
