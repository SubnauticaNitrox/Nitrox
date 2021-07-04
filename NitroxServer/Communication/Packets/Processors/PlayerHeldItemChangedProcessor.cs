using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class PlayerHeldItemChangedProcessor : AuthenticatedPacketProcessor<PlayerHeldItemChanged>
    {
        private readonly PlayerManager playerManager;

        public PlayerHeldItemChangedProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerHeldItemChanged packet, Player player)
        {
            if (packet.IsFirstTime != null && !player.UsedItems.Contains(packet.IsFirstTime))
            {
                player.UsedItems.Add(packet.IsFirstTime);
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
