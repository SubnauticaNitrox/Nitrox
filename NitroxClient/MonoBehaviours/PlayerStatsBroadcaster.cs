using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerStatsBroadcaster : MonoBehaviour
{
    private float time;
    private const float BROADCAST_INTERVAL = 3f;
    private LocalPlayer localPlayer;
    private Survival survival;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();
        survival = Player.main.GetComponent<Survival>();
    }

    public void Update()
    {
        time += Time.deltaTime;

        // Only do on a specific cadence to avoid hammering server
        if (time >= BROADCAST_INTERVAL)
        {
            time = 0;

            if (survival)
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
