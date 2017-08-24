using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
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
            Optional<GameObject> opOwner = GuidHelper.GetObjectFrom(packet.OwnerGuid);
            Optional<GameObject> opItem = GuidHelper.GetObjectFrom(packet.ItemGuid);

            if (opOwner.IsPresent() && opItem.IsPresent())
            {
                GameObject owner = opOwner.Get();
                GameObject item = opItem.Get();

                Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner);

                if (opContainer.IsPresent())
                {
                    ItemsContainer container = opContainer.Get();
                    Pickupable pickupable = item.GetComponent<Pickupable>();

                    if (pickupable != null)
                    {
                        using (packetSender.Suppress<ItemContainerRemove>())
                        {
                            container.RemoveItem(pickupable, true);
                        }
                    }
                    else
                    {
                        Console.WriteLine(item.name + " did not have a corresponding pickupable script!");
                    }
                }
                else
                {
                    Console.WriteLine("Could not find container field on object " + owner.name);
                }
            }
            else
            {
                Console.WriteLine("Owner or item was not found: " + opOwner + " " + opItem + " " + packet.OwnerGuid + " " + packet.ItemGuid);
            }
        }
    }
}
