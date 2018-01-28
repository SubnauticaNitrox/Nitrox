using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures.Util;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ItemContainerAddProcessor : ClientPacketProcessor<ItemContainerAdd>
    {
        private readonly IPacketSender packetSender;

        public ItemContainerAddProcessor(IPacketSender packetSender)
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
                
                using (packetSender.suppress<ItemContainerAdd>())
                {
                    container.UnsafeAdd(new InventoryItem(pickupable));
                }
            }
            else
            {
                Log.Error("Could not find container field on object " + owner.name);
            }
        }
    }
}
