using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic.HUD;
using Nitrox.Client.MonoBehaviours.Gui.HUD;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    class PlayerStatsProcessor : ClientPacketProcessor<PlayerStats>
    {
        private readonly PlayerVitalsManager vitalsManager;

        public PlayerStatsProcessor(PlayerVitalsManager vitalsManager)
        {
            this.vitalsManager = vitalsManager;
        }

        public override void Process(PlayerStats playerStats)
        {
            RemotePlayerVitals vitals = vitalsManager.CreateForPlayer(playerStats.PlayerId);

            vitals.SetOxygen(playerStats.Oxygen, playerStats.MaxOxygen);
            vitals.SetHealth(playerStats.Health);
            vitals.SetFood(playerStats.Food);
            vitals.SetWater(playerStats.Water);
        }
    }
}
