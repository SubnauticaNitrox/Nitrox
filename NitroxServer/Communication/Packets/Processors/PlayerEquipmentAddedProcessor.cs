using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
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
