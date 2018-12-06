using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleInternalLightingProcessor : ClientPacketProcessor<CyclopsToggleInternalLighting>
    {
        private readonly IPacketSender packetSender;

        public CyclopsToggleInternalLightingProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsToggleInternalLighting lightingPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(lightingPacket.Guid);
            CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();

            if (lighting.lightingOn != lightingPacket.IsOn)
            {
                using (packetSender.Suppress<CyclopsToggleInternalLighting>())
                {
                    lighting.ToggleInternalLighting();
                }
            }
        }
    }
}
