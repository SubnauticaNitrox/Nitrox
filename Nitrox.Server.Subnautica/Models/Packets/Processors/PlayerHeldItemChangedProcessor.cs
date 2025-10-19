using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
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
