using Nitrox.Model.DataStructures.Unity;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

/// <summary>
///     Related to DeathMarker server setting.
/// </summary>
internal sealed class DeathBeacon : MonoBehaviour
{
    private const float DESPAWN_DISTANCE = 20f;

    private void OnTriggerEnter(Collider collider)
    {
        if (!collider.gameObject.IsLocalPlayer)
        {
            return;
        }

        Log.Debug($"{nameof(DeathBeacon)} '{name}' despawn trigger entered by {collider.name}");
        Destroy(gameObject);
    }

    public static void SpawnDeathBeacon(NitroxVector3 location, string playerName)
    {
        GameObject beacon = new($"{playerName}{nameof(DeathBeacon)}");
        beacon.transform.position = location.ToUnity();
        beacon.layer = LayerID.Trigger | LayerID.OnlyVehicle;
        beacon.AddComponent<DeathBeacon>();
        PingInstance signal = beacon.AddComponent<PingInstance>();
        signal.IsLocalOnly = true;
        signal.pingType = PingType.Signal;
        signal.origin = beacon.transform;
        signal.minDist = DESPAWN_DISTANCE + 15f;
        signal._label = Language.main.Get("Nitrox_PlayerDeathBeaconLabel").Replace("{PLAYER}", playerName);
        signal.displayPingInManager = true;
        signal.Initialize();
        SphereCollider collider = beacon.AddComponent<SphereCollider>();
        collider.radius = DESPAWN_DISTANCE;
        collider.isTrigger = true;
    }
}
