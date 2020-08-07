using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class EnergyMixinValueChangedPacketProcessor : AuthenticatedPacketProcessor<EnergyMixinValueChanged>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryManager inventoryManager;

        public EnergyMixinValueChangedPacketProcessor(PlayerManager playerManager, InventoryManager inventoryManager)
        {
            this.playerManager = playerManager;
            this.inventoryManager = inventoryManager;
        }

        public override void Process(EnergyMixinValueChanged packet, Player player)
        {
            if (!inventoryManager.GetAllStorageSlotItems().Contains(packet.BatteryData))
            {
                inventoryManager.StorageItemAdded(packet.BatteryData); // Updates the charge of the battery on the server
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
