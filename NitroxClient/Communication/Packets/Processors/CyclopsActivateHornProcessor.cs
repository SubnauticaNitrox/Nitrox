using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsActivateHornProcessor : ClientPacketProcessor<CyclopsActivateHorn>
    {
        private readonly IPacketSender packetSender;

        public CyclopsActivateHornProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateHorn hornPacket)
        {
            GameObject cyclops = NitroxIdentifier.RequireObjectFrom(hornPacket.Id);
            CyclopsHornControl horn = cyclops.RequireComponentInChildren<CyclopsHornControl>();

            Utils.PlayEnvSound(horn.hornSound, horn.hornSound.gameObject.transform.position, 20f);
        }
    }
}
