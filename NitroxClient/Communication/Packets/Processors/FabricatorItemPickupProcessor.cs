using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FabricatorItemPickupProcessor : ClientPacketProcessor<FabricatorItemPickup>
    {
        private PacketSender packetSender;

        public FabricatorItemPickupProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(FabricatorItemPickup packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.FabricatorGuid);
            CrafterLogic crafterLogic = gameObject.GetComponentInChildren<CrafterLogic>(true);

            if (crafterLogic == null)
            {
                Console.WriteLine("Game object did not have a crafterLogic component!");
                return;
            }
            
            if(crafterLogic.numCrafted > 0)
            {
                crafterLogic.numCrafted--;

                if(crafterLogic.numCrafted == 0)
                {
                    crafterLogic.Reset();
                } 
            }
        }
    }
}
