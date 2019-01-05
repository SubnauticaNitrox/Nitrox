using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Communication.Packets.Processors
{
    class EquipmentAddItemPacketProcessor : AuthenticatedPacketProcessor<EquipmentAddItem>
    {
        private readonly PlayerManager playerManager;
        private readonly PlayerData playerData;

        public EquipmentAddItemPacketProcessor(PlayerManager playerManager, PlayerData playerData)
        {
            this.playerManager = playerManager;
            this.playerData = playerData;
        }

        public override void Process(EquipmentAddItem packet, Player player)
        {

            if (packet.IsPlayerEquipment)
            {
                playerData.AddEquipment(player.Name, packet.EquippedItemData);
            }
            else
            {
                playerData.AddModule(packet.EquippedItemData);
            }
            
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
