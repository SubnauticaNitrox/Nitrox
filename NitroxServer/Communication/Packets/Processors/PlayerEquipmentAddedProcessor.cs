using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerEquipmentAddedProcessor : AuthenticatedPacketProcessor<PlayerEquipmentAdded>
    {
        private readonly PlayerManager playerManager;
        private readonly PlayerData playerData;

        public PlayerEquipmentAddedProcessor(PlayerManager playerManager, PlayerData playerData)
        {
            this.playerManager = playerManager;
            this.playerData = playerData;
        }

        public override void Process(PlayerEquipmentAdded packet, Player player)
        {
            string playerName = player.Name;
            EquippedItemData equippedItem = packet.EquippedItem;

            playerData.AddEquipment(playerName, equippedItem);

            ushort playerId = player.Id;
            TechType techType = packet.TechType;
            RemotePlayerEquipmentAdded equipmentAdded = new RemotePlayerEquipmentAdded(playerId, techType);

            playerManager.SendPacketToOtherPlayers(equipmentAdded, player);
        }
    }
}
