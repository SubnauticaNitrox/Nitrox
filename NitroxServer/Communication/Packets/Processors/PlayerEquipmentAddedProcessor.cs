using NitroxModel.DataStructures;
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
            
            RemotePlayerEquipmentAdded equipmentAdded = new RemotePlayerEquipmentAdded(player.Id, packet.TechType);

            playerManager.SendPacketToOtherPlayers(equipmentAdded, player);
        }
    }
}
