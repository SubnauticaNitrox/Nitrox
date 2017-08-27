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

        public CyclopsActivateHornProcessor(PacketSender packetSender) {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateHorn packet) {
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(packet.Guid);

            if (opCyclops.IsPresent()) {
                CyclopsHornControl horn = opCyclops.Get().GetComponentInChildren<CyclopsHornControl>();

                if (horn != null) {
                    using (packetSender.Suppress<CyclopsActivateHorn>()) {
                        Utils.PlayEnvSound(horn.hornSound, horn.hornSound.gameObject.transform.position, 20f);
                    }
                } else {
                    Console.WriteLine("Could not begin silent running because CyclopsHornButton was not found on the cyclops " + packet.Guid);
                }
            } else {
                Console.WriteLine("Could not find cyclops with guid " + packet.Guid + " to activate horn.");
            }
        }
    }
}
