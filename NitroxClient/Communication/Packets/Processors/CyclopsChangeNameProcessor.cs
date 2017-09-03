using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeNameProcessor : ClientPacketProcessor<CyclopsChangeName>
    {
        private PacketSender packetSender;

        public CyclopsChangeNameProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeName namePacket)
        {
            Optional<GameObject> opCyclops = GuidHelper.GetObjectFrom(namePacket.Guid);

            SubName subname = (SubName)opCyclops.Get().GetComponentInChildren<CyclopsNameScreenProxy>().subNameInput.ReflectionGet("target"); ;

            if (opCyclops.IsPresent())
            {
                subname.SetName(namePacket.Name);
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + namePacket.Guid + " to change name");
            }
        }
    }
}
