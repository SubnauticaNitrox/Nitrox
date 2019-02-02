using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerEquipmentRemovedProcessor : AuthenticatedPacketProcessor<PlayerEquipmentRemoved>
    {
        private readonly PlayerManager playerManager;
        private readonly PlayerData playerData;

        public PlayerEquipmentRemovedProcessor(PlayerManager playerManager, PlayerData playerData)
        {
            this.playerManager = playerManager;
            this.playerData = playerData;
        }

        public override void Process(PlayerEquipmentRemoved packet, Player player)
        {
            string playerName = player.Name;
            string itemGuid = packet.EquippedItemGuid;

            playerData.RemoveEquipment(playerName, itemGuid);

            ushort playerId = player.Id;
            TechType techType = packet.TechType;
            RemotePlayerEquipmentRemoved equipmentRemoved = new RemotePlayerEquipmentRemoved(playerId, techType);

            playerManager.SendPacketToOtherPlayers(equipmentRemoved, player);
        }
    }
}
