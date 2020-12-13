using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class PlayerEquipmentAddedProcessor : AuthenticatedPacketProcessor<PlayerEquipmentAdded>
    {
        private readonly PlayerManager playerManager;

        public PlayerEquipmentAddedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerEquipmentAdded packet, Player player)
        {
            string playerName = player.Name;
            EquippedItemData equippedItem = packet.EquippedItem;

            player.AddEquipment(equippedItem);

            ushort playerId = player.Id;
            RemotePlayerEquipmentAdded equipmentAdded = new RemotePlayerEquipmentAdded(playerId, packet.TechType);

            playerManager.SendPacketToOtherPlayers(equipmentAdded, player);
        }
    }
}
