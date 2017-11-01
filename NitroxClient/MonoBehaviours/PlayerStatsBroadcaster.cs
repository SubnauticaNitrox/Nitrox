using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerStatsBroadcaster : MonoBehaviour
    {
        private float time = 0.0f;
        private const float INTERPOLATION_PERIOD = 4.00f;

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= INTERPOLATION_PERIOD)
            {
                time = 0;

                Survival survival = Player.main.GetComponent<Survival>();

                if (survival != null && !survival.freezeStats && GameModeConsoleCommands.main && GameModeConsoleCommands.main.GetSurvivalEnabled())
                {
                    float oxygen = Player.main.oxygenMgr.GetOxygenAvailable();
                    float maxOxygen = Player.main.oxygenMgr.GetOxygenCapacity();
                    float health = Player.main.liveMixin.health;
                    float food = survival.food;
                    float water = survival.water;

                    Multiplayer.Logic.PlayerAttributes.BroadcastPlayerStats(oxygen, maxOxygen, health, food, water);
                }
            }
        }
    }
}
