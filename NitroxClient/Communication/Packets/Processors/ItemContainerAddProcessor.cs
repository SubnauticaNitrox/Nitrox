using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
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
            Optional<GameObject> opOwner = GuidHelper.GetObjectFrom(packet.OwnerGuid);

            if (opOwner.IsPresent())
            {
                GameObject owner = opOwner.Get();

                Optional<ItemsContainer> opContainer = InventoryContainerHelper.GetBasedOnOwnersType(owner);

                if (opContainer.IsPresent())
                {
                    ItemsContainer container = opContainer.Get();
                    GameObject item = SerializationHelper.GetGameObject(packet.ItemData);
                    Pickupable pickupable = item.GetComponent<Pickupable>();

                    if (pickupable != null)
                    {
                        using (packetSender.Suppress<ItemContainerAdd>())
                        {
                            container.UnsafeAdd(new InventoryItem(pickupable));
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
                Console.WriteLine("Could not find owner with guid: " + packet.OwnerGuid);
            }
        }
    }
}
