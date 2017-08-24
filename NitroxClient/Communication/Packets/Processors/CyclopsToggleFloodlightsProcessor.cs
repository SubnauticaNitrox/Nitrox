using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleFloodlightsProcessor : ClientPacketProcessor<CyclopsToggleFloodLights>
    {
        private PacketSender packetSender;

        public CyclopsToggleFloodlightsProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsToggleFloodLights lightingPacket)
        {
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(lightingPacket.Guid);

            if (opCyclops.IsPresent())
            {
                CyclopsLightingPanel lighting = opCyclops.Get().GetComponentInChildren<CyclopsLightingPanel>();

                if (lighting.floodlightsOn != lightingPacket.IsOn)
                {
                    using (packetSender.Suppress<CyclopsToggleFloodLights>())
                    {
                        lighting.ToggleFloodlights();
                    }
                }
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + lightingPacket.Guid + " to change flood lighting");
            }
        }
    }
}
