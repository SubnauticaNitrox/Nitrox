using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleFloodlightsProcessor : ClientPacketProcessor<CyclopsToggleFloodLights>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsToggleFloodlightsProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsToggleFloodLights lightingPacket)
        {
            cyclops.SetFloodLighting(lightingPacket.Id, lightingPacket.IsOn);
        }
    }
}
