using System.Collections;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

public class DeathBeacon : MonoBehaviour
{
    private const float despawnDistance = 20f;

    public static IEnumerator SpawnDeathBeacon(NitroxVector3 location, string playerName)
    {
        GameObject beacon = new($"{playerName}DeathBeacon");
        PingInstance signal = beacon.AddComponent<PingInstance>();
        signal.pingType = PingType.Signal;
        signal.displayPingInManager = true;
        signal.origin = beacon.transform;
        signal._label = $"{playerName}'s death";
        beacon.transform.position = location.ToUnity();
        beacon.AddComponent<DeathBeacon>();
        signal.Initialize();
        yield break;
    }

    private void Update()
    {
        if (Vector3.Distance(Player.main.transform.position, transform.position) <= despawnDistance)
        {
            Destroy(gameObject);
        }
    }
}
