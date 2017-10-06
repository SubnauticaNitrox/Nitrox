using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleInternalLightingProcessor : ClientPacketProcessor<CyclopsToggleInternalLighting>
    {
        private readonly PacketSender packetSender;

        public CyclopsToggleInternalLightingProcessor(PacketSender packetSender)
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
