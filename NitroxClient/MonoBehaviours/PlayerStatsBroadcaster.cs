using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerStatsBroadcaster : MonoBehaviour
    {
        private float time;
        private const float INTERPOLATION_PERIOD = 4.00f;
        private LocalPlayer localPlayer;

        public void Awake()
        {
            localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
        }

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= INTERPOLATION_PERIOD)
            {
                time = 0;
                Survival survival = Player.main.GetComponent<Survival>();

                if (survival != null && !survival.freezeStats)
                {
                    float oxygen = Player.main.oxygenMgr.GetOxygenAvailable();
                    float maxOxygen = Player.main.oxygenMgr.GetOxygenCapacity();
                    float health = Player.main.liveMixin.health;
                    float food = survival.food;
                    float water = survival.water;
                    float infectionAmount = Player.main.infectedMixin.GetInfectedAmount();
                    localPlayer.BroadcastStats(oxygen, maxOxygen, health, food, water, infectionAmount);
                }
            }
        }
    }
}
