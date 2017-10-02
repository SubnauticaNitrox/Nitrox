using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsActivateShieldProcessor : ClientPacketProcessor<CyclopsActivateShield>
    {
        private PacketSender packetSender;

        public CyclopsActivateShieldProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateShield shieldPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(shieldPacket.Guid);            
            CyclopsShieldButton shield = cyclops.GetComponentInChildren<CyclopsShieldButton>();

            if (shield != null)
            {
                using (packetSender.Suppress<CyclopsActivateShield>())
                {
                    shield.OnClick();
                }
            }
            else
            {
                Console.WriteLine("Could not activate shield because CyclopsShieldButton was not found on the cyclops " + shieldPacket.Guid);
            }
        }
    }
}
