using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerStatsBroadcaster : MonoBehaviour
    {
        private float time = 0.0f;
        public float interpolationPeriod = 4.00f;

        public void Update()
        {
            time += Time.deltaTime;

            // Only do on a specific cadence to avoid hammering server
            if (time >= interpolationPeriod)
            {
                time = 0;

                Survival survivial = Player.main.GetComponent<Survival>();

                if (survivial != null)
                {
                    float oxygen = Player.main.oxygenMgr.GetOxygenAvailable();
                    float maxOxygen = Player.main.oxygenMgr.GetOxygenCapacity();
                    float health = Player.main.liveMixin.health;
                    float food = survivial.food;
                    float water = survivial.water;

                    Multiplayer.Logic.PlayerAttributes.BroadcastPlayerStats(oxygen, maxOxygen, health, food, water);
                }
            }
        }
    }
}
