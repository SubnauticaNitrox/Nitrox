using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleInternalLightingProcessor : ClientPacketProcessor<CyclopsToggleInternalLighting>
    {
        private readonly IPacketSender packetSender;
        private readonly Cyclops cyclops;

        public CyclopsToggleInternalLightingProcessor(IPacketSender packetSender, Cyclops cyclops)
        {
            this.packetSender = packetSender;
            this.cyclops = cyclops;
        }

        public override void Process(CyclopsToggleInternalLighting lightingPacket)
        {
            cyclops.SetInternalLighting(lightingPacket.Id, lightingPacket.IsOn);
        }
    }
}
