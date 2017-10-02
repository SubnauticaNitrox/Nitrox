using NitroxClient.Communication.Packets.Processors.Abstract;
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
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.Guid);            
            Openable openable = gameObject.GetComponent<Openable>();

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
    }
}
