using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
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
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(shieldPacket.Guid);
            
            if (opCyclops.IsPresent())
            {
                CyclopsShieldButton shield = opCyclops.Get().GetComponentInChildren<CyclopsShieldButton>();

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
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + shieldPacket.Guid + " to activate shield.");
            }
        }
    }
}
