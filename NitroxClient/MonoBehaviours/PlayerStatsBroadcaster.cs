using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerStatsBroadcaster : MonoBehaviour
    {
        private float time;
        private const float INTERPOLATION_PERIOD = 4.00f;
        private PlayerLogic playerBroadcaster;

        public void Awake()
        {
            playerBroadcaster = NitroxServiceLocator.LocateService<PlayerLogic>();
        }

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= INTERPOLATION_PERIOD)
            {
                time = 0;

                Survival survival = Player.main.GetComponent<Survival>();

                if (survival != null && !survival.freezeStats && GameModeUtils.IsOptionActive(GameModeOption.Survival))
                {
                    float oxygen = Player.main.oxygenMgr.GetOxygenAvailable();
                    float maxOxygen = Player.main.oxygenMgr.GetOxygenCapacity();
                    float health = Player.main.liveMixin.health;
                    float food = survival.food;
                    float water = survival.water;

                    playerBroadcaster.BroadcastStats(oxygen, maxOxygen, health, food, water);
                }
            }
        }
    }
}
