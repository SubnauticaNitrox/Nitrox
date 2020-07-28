using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleInternalLightingProcessor : ClientPacketProcessor<CyclopsToggleInternalLighting>
    {
        private readonly Cyclops cyclops;

        public CyclopsToggleInternalLightingProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsToggleInternalLighting lightingPacket)
        {
            cyclops.SetInternalLighting(lightingPacket.Id, lightingPacket.IsOn);
        }
    }
}
