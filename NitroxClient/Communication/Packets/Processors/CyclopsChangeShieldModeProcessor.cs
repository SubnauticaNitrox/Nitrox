using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeShieldModeProcessor : ClientPacketProcessor<CyclopsChangeShieldMode>
    {
        private readonly IPacketSender packetSender;

        public CyclopsChangeShieldModeProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeShieldMode shieldPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(shieldPacket.Guid);
            CyclopsShieldButton shield = cyclops.RequireComponentInChildren<CyclopsShieldButton>();

            using (packetSender.Suppress<CyclopsChangeShieldMode>())
            {
                if ((shield.activeSprite == shield.image.sprite) != shieldPacket.IsOn)
                {
                    if (shieldPacket.IsOn)
                    {
                        shield.ReflectionCall("StartShield");
                    } else
                    {
                        shield.ReflectionCall("StopShield");
                    }
                }
            }
        }
    }
}
