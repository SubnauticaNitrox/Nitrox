using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
