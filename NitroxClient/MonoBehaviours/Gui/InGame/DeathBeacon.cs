using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

public class DeathBeacon : MonoBehaviour
{
    private const float DESPAWN_DISTANCE = 20f;
    private const float DESPAWN_DISTANCE_SQUARED = DESPAWN_DISTANCE * DESPAWN_DISTANCE;
    private const float CHECK_RATE = 1.5f; // in seconds

    public static void SpawnDeathBeacon(NitroxVector3 location, string playerName)
    {
        GameObject beacon = new($"{playerName}DeathBeacon");
        beacon.transform.position = location.ToUnity();
        PingInstance signal = beacon.AddComponent<PingInstance>();
        signal.pingType = PingType.Signal;
        signal.origin = beacon.transform;
        signal.minDist = DESPAWN_DISTANCE + 15f;
        signal._label = Language.main.Get("Nitrox_PlayerDeathBeaconLabel").Replace("{PLAYER}", playerName);
        beacon.AddComponent<DeathBeacon>();
        signal.displayPingInManager = true;
        signal.Initialize();
    }

    private void Start()
    {
        InvokeRepeating(nameof(CheckPlayerDistance), 0, CHECK_RATE);
    }

    private void CheckPlayerDistance()
    {
        if ((Player.main.transform.position - transform.position).sqrMagnitude <= DESPAWN_DISTANCE_SQUARED)
        {
            Destroy(gameObject);
        }
    }
}
