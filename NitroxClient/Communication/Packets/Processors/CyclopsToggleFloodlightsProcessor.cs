using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleFloodlightsProcessor : ClientPacketProcessor<CyclopsToggleFloodLights>
    {
        private readonly PacketSender packetSender;

        public CyclopsToggleFloodlightsProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsToggleFloodLights lightingPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(lightingPacket.Guid);            
            CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();

            if (lighting.floodlightsOn != lightingPacket.IsOn)
            {
                using (packetSender.Suppress<CyclopsToggleFloodLights>())
                {
                    lighting.ToggleFloodlights();
                }
            }
        }
    }
}
