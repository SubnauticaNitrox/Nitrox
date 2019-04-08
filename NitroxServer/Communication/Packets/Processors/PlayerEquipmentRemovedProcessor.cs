using NitroxModel.DataStructures;
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
            NitroxId itemId = packet.EquippedItemId;

            playerData.RemoveEquipment(playerName, itemId);

            ushort playerId = player.Id;
            RemotePlayerEquipmentRemoved equipmentRemoved = new RemotePlayerEquipmentRemoved(playerId, packet.TechType);

            playerManager.SendPacketToOtherPlayers(equipmentRemoved, player);
        }
    }
}
