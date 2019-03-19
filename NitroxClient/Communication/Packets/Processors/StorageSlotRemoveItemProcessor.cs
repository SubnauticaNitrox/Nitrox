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
    class StorageSlotRemoveItemProcessor : ClientPacketProcessor<StorageSlotItemRemove>
    {
        private readonly IPacketSender packetSender;
        private readonly StorageSlots storageSlots;

        public StorageSlotRemoveItemProcessor(IPacketSender packetSender, StorageSlots storageSlots)
        {
            this.packetSender = packetSender;
            this.storageSlots = storageSlots;
        }

        public override void Process(StorageSlotItemRemove packet)
        {
            Log.Debug("Process storage slot remove");
            storageSlots.RemoveItem(packet.OwnerGuid);
            Log.Debug("Process storage slot remove finished");
        }
    }
}
