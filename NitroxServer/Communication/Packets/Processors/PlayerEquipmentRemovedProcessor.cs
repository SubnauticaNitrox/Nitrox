using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerEquipmentRemovedProcessor : AuthenticatedPacketProcessor<PlayerEquipmentRemoved>
    {
        private readonly PlayerManager playerManager;

        public PlayerEquipmentRemovedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerEquipmentRemoved packet, Player player)
        {
            string playerName = player.Name;
            NitroxId itemId = packet.EquippedItemId;

            player.RemoveEquipment(itemId);

            ushort playerId = player.Id;
            RemotePlayerEquipmentRemoved equipmentRemoved = new RemotePlayerEquipmentRemoved(playerId, packet.TechType);

            playerManager.SendPacketToOtherPlayers(equipmentRemoved, player);
        }
    }
}
