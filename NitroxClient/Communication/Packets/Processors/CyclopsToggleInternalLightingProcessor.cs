using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

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
            cyclops.SetInternalLighting(lightingPacket.Guid, lightingPacket.IsOn);
        }
    }
}
