using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class PlayerStatsProcessor : AuthenticatedPacketProcessor<PlayerStats>
    {
        private readonly PlayerManager playerManager;

        public PlayerStatsProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(PlayerStats packet, Player player)
        {
            player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
