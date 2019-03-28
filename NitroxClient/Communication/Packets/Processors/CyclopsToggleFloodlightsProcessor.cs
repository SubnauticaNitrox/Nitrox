using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsToggleFloodlightsProcessor : ClientPacketProcessor<CyclopsToggleFloodLights>
    {
        private readonly IPacketSender packetSender;

        public CyclopsToggleFloodlightsProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsToggleFloodLights lightingPacket)
        {
            NitroxServiceLocator.LocateService<Cyclops>().SetFloodLighting(lightingPacket.Guid, lightingPacket.IsOn);
        }
    }
}
