using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.HUD;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    class PlayerStatsProcessor : ClientPacketProcessor<PlayerStats>
    {
        private readonly PlayerVitalsManager vitalsManager;
        private readonly PlayerManager playerManager;

        public PlayerStatsProcessor(PlayerVitalsManager vitalsManager, PlayerManager playerManager)
        {
            this.vitalsManager = vitalsManager;
            this.playerManager = playerManager;
        }

        public override void Process(PlayerStats playerStats)
        {
            RemotePlayerVitals vitals = vitalsManager.GetForPlayerId(playerStats.PlayerId, playerManager);

            vitals.SetOxygen(playerStats.Oxygen, playerStats.MaxOxygen);
            vitals.SetHealth(playerStats.Health);
            vitals.SetFood(playerStats.Food);
            vitals.SetWater(playerStats.Water);
        }
    }
}
