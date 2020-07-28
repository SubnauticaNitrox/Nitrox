using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeShieldModeProcessor : ClientPacketProcessor<CyclopsChangeShieldMode>
    {
        private readonly Cyclops cyclops;

        public CyclopsChangeShieldModeProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsChangeShieldMode shieldPacket)
        {
            cyclops.ChangeShieldMode(shieldPacket.Id, shieldPacket.IsOn);
        }
    }
}
