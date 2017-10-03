using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerRemoveProcessor : ClientPacketProcessor<ItemContainerRemove>
    {
        private PacketSender packetSender;

        public ItemContainerRemoveProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(ItemContainerRemove packet)
        {
            GameObject owner = GuidHelper.RequireObjectFrom(packet.OwnerGuid);
            GameObject item = GuidHelper.RequireObjectFrom(packet.ItemGuid);
            Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner);

            if (opContainer.IsPresent())
            {
                ItemsContainer container = opContainer.Get();
                Pickupable pickupable = item.RequireComponent<Pickupable>();
                
                using (packetSender.Suppress<ItemContainerRemove>())
                {
                    container.RemoveItem(pickupable, true);
                }
            }
            else
            {
                Console.WriteLine("Could not find container field on object " + owner.name);
            }
        }
    }
}
