﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
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
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.FabricatorGuid);

            if (opGameObject.IsEmpty())
            {
                Console.WriteLine("Could not find fabricator from guid " + packet.FabricatorGuid);
                return;
            }

            GameObject gameObject = opGameObject.Get();
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
