using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

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
            storageSlots.RemoveItem(packet.OwnerId);
        }
    }
}
