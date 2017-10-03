using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
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
            GameObject cyclops = GuidHelper.RequireObjectFrom(namePacket.Guid);
            CyclopsNameScreenProxy ScreenProxy = cyclops.RequireComponentInChildren<CyclopsNameScreenProxy>();            
            SubName subname = (SubName)ScreenProxy.subNameInput.ReflectionGet("target");

            if (subname != null)
            {
                subname.SetName(namePacket.Name);
                ScreenProxy.subNameInput.inputField.text = namePacket.Name;
            }
            else
            {
                Console.WriteLine("Could not find SubName via target reflection");
            } 
        }
    }
}
