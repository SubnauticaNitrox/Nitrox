using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
            ItemData itemData = packet.ItemData;
            GameObject item = SerializationHelper.GetGameObject(itemData.SerializedData);            
            
            NitroxEntity.SetNewId(item, itemData.ItemId);

            storageSlots.AddItem(item, itemData.ContainerId);            
        }
    }
}
