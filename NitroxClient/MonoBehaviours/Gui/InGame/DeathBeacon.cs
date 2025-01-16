using System.Collections;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

public class DeathBeacon : MonoBehaviour
{
    private const float despawnDistance = 20f;
    private const float despawnDistanceSquared = despawnDistance * despawnDistance;
    private const float checkRate = 10f; // in seconds

    public static IEnumerator SpawnDeathBeacon(NitroxVector3 location, string playerName)
    {
        GameObject beacon = new($"{playerName}DeathBeacon");
        PingInstance signal = beacon.AddComponent<PingInstance>();
        signal.pingType = PingType.Signal;
        signal.displayPingInManager = true;
        signal.origin = beacon.transform;
        signal._label = Language.main.Get("Nitrox_PlayerDeathBeaconLabel").Replace("{PLAYER}", playerName);
        beacon.transform.position = location.ToUnity();
        beacon.AddComponent<DeathBeacon>();
        signal.Initialize();
        yield break;
    }

    private void Start()
    {
        InvokeRepeating("CheckPlayerDistance", 0, checkRate);
    }
    private void CheckPlayerDistance()
    {
        if ((Player.main.transform.position - transform.position).sqrMagnitude <= despawnDistanceSquared)
        {
            Destroy(gameObject);
        }
    }
}
