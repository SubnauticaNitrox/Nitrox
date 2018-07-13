using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Communication.Packets.Processors
{
    class EquipmentRemoveItemPacketProcessor : AuthenticatedPacketProcessor<EquipmentRemoveItem>
    {
        private readonly PlayerManager playerManager;
        private readonly PlayerData playerData;

        public EquipmentRemoveItemPacketProcessor(PlayerManager playerManager, PlayerData playerData)
        {
            this.playerManager = playerManager;
            this.playerData = playerData;
        }

        public override void Process(EquipmentRemoveItem packet, Player player)
        {
            if (packet.IsPlayerEquipment)
            {
                playerData.RemoveEquipment(player.Name, packet.ItemGuid);
            }
            else
            {
                playerData.RemoveModule(packet.ItemGuid);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
