using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class StorageSlotAddItemProcessor : ClientPacketProcessor<StorageSlotItemAdd>
    {
        private readonly StorageSlots storageSlots;

        public StorageSlotAddItemProcessor(StorageSlots storageSlots)
        {
            this.storageSlots = storageSlots;
        }

        public override void Process(StorageSlotItemAdd packet)
        {
            ItemData itemData = packet.ItemData;
            GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);

            NitroxEntity.SetNewId(item, itemData.ItemId);

            storageSlots.AddItem(item, itemData.ContainerId);
        }
    }
}
