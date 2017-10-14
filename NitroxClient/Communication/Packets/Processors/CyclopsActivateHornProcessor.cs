using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsActivateHornProcessor : ClientPacketProcessor<CyclopsActivateHorn>
    {
        private PacketSender packetSender;

        public CyclopsActivateHornProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateHorn hornPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(hornPacket.Guid);            
            CyclopsHornControl horn = cyclops.RequireComponent<CyclopsHornControl>();

            Utils.PlayEnvSound(horn.hornSound, horn.hornSound.gameObject.transform.position, 20f);
        }
    }
}
