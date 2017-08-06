using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class PlayerStatsProcessor : ClientPacketProcessor<PlayerStats>
    {
        private PlayerVitalsManager vitalsManager;

        public PlayerStatsProcessor(PlayerVitalsManager vitalsManager)
        {
            this.vitalsManager = vitalsManager;
        }

        public override void Process(PlayerStats playerStats)
        {
            RemotePlayerVitals vitals = vitalsManager.GetForPlayerId(playerStats.PlayerId);

            vitals.SetOxygen(playerStats.Oxygen, playerStats.MaxOxygen);
            vitals.SetHealth(playerStats.Health);
            vitals.SetFood(playerStats.Food);
            vitals.SetWater(playerStats.Water);
        }
    }
}
