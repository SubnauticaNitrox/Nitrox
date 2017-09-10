using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
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

            if (opCyclops.IsPresent())
            {
                CyclopsNameScreenProxy ScreenProxy = opCyclops.Get().GetComponentInChildren<CyclopsNameScreenProxy>();

                if (ScreenProxy != null)
                {
                    SubName subname = (SubName)ScreenProxy.subNameInput.ReflectionGet("target");

                    if (subname != null)
                    {
                        subname.SetName(namePacket.Name);
                        ScreenProxy.subNameInput.inputField.text = namePacket.Name;
                    }
                }
            }
            else
            {
                Console.WriteLine("Could not find cyclops with guid " + namePacket.Guid + " to change name");
            }
        }
    }
}
