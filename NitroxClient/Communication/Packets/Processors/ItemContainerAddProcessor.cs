using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerAddProcessor : ClientPacketProcessor<ItemContainerAdd>
    {
        private PacketSender packetSender;

        public ItemContainerAddProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(ItemContainerAdd packet)
        {
            GameObject owner = GuidHelper.RequireObjectFrom(packet.OwnerGuid);            
            Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner);

            if (opContainer.IsPresent())
            {
                ItemsContainer container = opContainer.Get();
                GameObject item = SerializationHelper.GetGameObject(packet.ItemData);
                Pickupable pickupable = item.RequireComponent<Pickupable>();
                
                using (packetSender.Suppress<ItemContainerAdd>())
                {
                    container.UnsafeAdd(new InventoryItem(pickupable));
                }
            }
            else
            {
                Console.WriteLine("Could not find container field on object " + owner.name);
            }
        }
    }
}
