using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class OpenableStateChangedProcessor : ClientPacketProcessor<OpenableStateChanged>
    {
        private PacketSender packetSender;

        public OpenableStateChangedProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(OpenableStateChanged packet)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.Guid);

            if (opGameObject.IsPresent())
            {
                Openable openable = opGameObject.Get().GetComponent<Openable>();

                if (openable != null)
                {
                    using (packetSender.Suppress<OpenableStateChanged>())
                    {
                        openable.PlayOpenAnimation(packet.IsOpen, packet.Duration);
                    }
                }
                else
                {
                    Console.WriteLine("Gameobject did not have a corresponding openable to change state!");
                }
            }
            else
            {
                Console.WriteLine("Could not find openable game object with guid: " + packet.Guid);
            }
        }
    }
}
