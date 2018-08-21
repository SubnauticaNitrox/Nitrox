﻿using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    class ItemContainerRemovePacketProcessor : AuthenticatedPacketProcessor<ItemContainerRemove>
    {
        private readonly PlayerManager playerManager;
        private readonly InventoryData inventoryData;

        public ItemContainerRemovePacketProcessor(PlayerManager playerManager, InventoryData inventoryData)
        {
            this.playerManager = playerManager;
            this.inventoryData = inventoryData;
        }

        public override void Process(ItemContainerRemove packet, Player player)
        {
            inventoryData.ItemRemoved(packet.ItemGuid);

            if (packet.OwnerGuid != player.Id.ToString())
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
