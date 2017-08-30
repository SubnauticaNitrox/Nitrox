using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
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
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(hornPacket.Guid);

            if (opCyclops.IsPresent())
            {
                CyclopsHornControl horn = opCyclops.Get().GetComponentInChildren<CyclopsHornControl>();

                if (horn != null)
                {
                    Utils.PlayEnvSound(horn.hornSound, horn.hornSound.gameObject.transform.position, 20f);
                }
                else
                {
                    Console.WriteLine("Could not activate the horn because CyclopsHornControl was not found on the cyclops " + hornPacket.Guid);
                }
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + hornPacket.Guid + " to activate horn.");
            }
        }
    }
}
