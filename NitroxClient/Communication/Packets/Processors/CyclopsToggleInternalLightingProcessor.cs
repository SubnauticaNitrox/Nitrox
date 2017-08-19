using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleInternalLightingProcessor : ClientPacketProcessor<CyclopsToggleInternalLighting>
    {
        private PacketSender packetSender;

        public CyclopsToggleInternalLightingProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsToggleInternalLighting lightingPacket)
        {
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(lightingPacket.Guid);

            if(opCyclops.IsPresent())
            {
                CyclopsLightingPanel lighting = opCyclops.Get().GetComponentInChildren<CyclopsLightingPanel>();
                
                if(lighting.lightingOn != lightingPacket.IsOn)
                {
                    packetSender.AddSuppressedPacketType(typeof(CyclopsToggleInternalLighting));
                    lighting.ToggleInternalLighting();
                    packetSender.RemoveSuppressedPacketType(typeof(CyclopsToggleInternalLighting));
                }
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + lightingPacket.Guid + " to change internal lighting");
            }
        }
    }
}
