using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class StorageSlotAddItemProcessor : ClientPacketProcessor<StorageSlotItemAdd>
    {
        private readonly IPacketSender packetSender;
        private readonly StorageSlots storageSlots;

        public StorageSlotAddItemProcessor(IPacketSender packetSender, StorageSlots storageSlots)
        {
            this.packetSender = packetSender;
            this.storageSlots = storageSlots;
        }

        public override void Process(StorageSlotItemAdd packet)
        {
            Log.Debug("Process storage slot add");
            ItemData itemData = packet.ItemData;
            GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);

            // Mark this entity as spawned by the server
            item.AddComponent<NitroxEntity>();
            item.SetNewGuid(itemData.Guid);

            storageSlots.AddItem(item, itemData.ContainerGuid);
            Log.Debug("Process storage slot add finished");
        }
    }
}
