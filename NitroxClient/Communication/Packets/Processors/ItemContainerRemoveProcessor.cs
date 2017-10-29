using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerRemoveProcessor : ClientPacketProcessor<ItemContainerRemove>
    {
        private readonly PacketSender packetSender;

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
                Log.Error("Could not find container field on object " + owner.name);
            }
        }
    }
}
