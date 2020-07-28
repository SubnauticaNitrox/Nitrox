using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleFloodlightsProcessor : ClientPacketProcessor<CyclopsToggleFloodLights>
    {
        private readonly Cyclops cyclops;

        public CyclopsToggleFloodlightsProcessor(Cyclops cyclops)
        {
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsToggleFloodLights lightingPacket)
        {
            cyclops.SetFloodLighting(lightingPacket.Id, lightingPacket.IsOn);
        }
    }
}
