using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic;

/// <summary>
/// Registers one stasis sphere per connected remote player, and syncs their behaviour.<br/>
/// Also syncs remote torpedo (of all types) shots and hits.
/// </summary>
public class BulletManager
{
    private readonly PlayerManager playerManager;

    // This only allows for one stasis sphere per player
    // (which is the normal capacity, but could be adapted for a mod letting multiple stasis spheres)
    private readonly Dictionary<ushort, StasisSphere> stasisSpherePerPlayerId = [];

    /// <summary>
    /// TechTypes of objects which should have a Vehicle MB
    /// </summary>
    private static readonly HashSet<TechType> preloadedVehicleTypes = [
        TechType.Seamoth, TechType.Exosuit
    ];

    private readonly Dictionary<TechType, GameObject> torpedoPrefabByTechType = [];

    private GameObject stasisSpherePrefab;

    public BulletManager(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public void ShootSeamothTorpedo(NitroxId bulletId, TechType techType, Vector3 position, Quaternion rotation, float speed, float lifeTime)
    {
        if (!torpedoPrefabByTechType.TryGetValue(techType, out GameObject prefab))
        {
            Log.ErrorOnce($"[{nameof(BulletManager)}] Received ShootSeamothTorpedo request with TechType: {techType} but no prefab was loaded for it");
            return;
        }

        GameObject torpedoClone = GameObjectHelper.SpawnFromPrefab(prefab, bulletId);
        // We mark it to be able to ignore events from remote bullets
        torpedoClone.AddComponent<RemotePlayerBullet>();

        // We cast it to Bullet to ensure we're calling the same method as in Vehicle.TorpedoShot
        Bullet seamothTorpedo = torpedoClone.GetComponent<SeamothTorpedo>();
        seamothTorpedo.Shoot(position, rotation, speed, lifeTime);
    }

    public void TorpedoHit(NitroxId bulletId, Vector3 position, Quaternion rotation)
    {
        // On the local player, the torpedo might have already exploded while the packet is received with latency.
        // Therefore we don't need to log a failed query of bulletId
        if (NitroxEntity.TryGetComponentFrom(bulletId, out SeamothTorpedo torpedo))
        {
            torpedo.tr.position = position;
            torpedo.tr.rotation = rotation;
            torpedo.OnHit(default);
            torpedo.Deactivate();
        }
    }

    public void TorpedoTargetAcquired(NitroxId bulletId, NitroxId targetId, Vector3 position, Quaternion rotation)
    {
        // The target object might not be findable in which case we'll just ignore it
        // because the explosion will still be moved to the right spot
        if (NitroxEntity.TryGetComponentFrom(bulletId, out SeamothTorpedo torpedo) &&
            NitroxEntity.TryGetObjectFrom(targetId, out GameObject targetObject))
        {
            torpedo.tr.position = position;
            torpedo.tr.rotation = rotation;
            // Stuff from SeamothTorpedo.RepeatingTargeting
            torpedo.homingTarget = targetObject;
            torpedo.CancelInvoke();
        }
    }

    public void ShootStasisSphere(ushort playerId, Vector3 position, Quaternion rotation, float speed, float lifeTime, float chargeNormalized)
    {
        StasisSphere cloneSphere = EnsurePlayerHasSphere(playerId);

        cloneSphere.Shoot(position, rotation, speed, lifeTime, chargeNormalized);
    }

    public void StasisSphereHit(ushort playerId, Vector3 position, Quaternion rotation, float chargeNormalized, float consumption)
    {
        StasisSphere cloneSphere = EnsurePlayerHasSphere(playerId);

        // Setup the sphere in case the shot was sent earlier
        cloneSphere.Shoot(position, rotation, 0, 0, chargeNormalized);
        // We override this field (set by .Shoot) with the right data
        cloneSphere._consumption = consumption;

        // Code from Bullet.Update when finding an object to hit
        cloneSphere._visible = true;
        cloneSphere.OnMadeVisible();
        cloneSphere.EnableField();
        cloneSphere.Deactivate();
    }

    private StasisSphere EnsurePlayerHasSphere(ushort playerId)
    {
        if (stasisSpherePerPlayerId.TryGetValue(playerId, out StasisSphere remoteSphere) && remoteSphere)
        {
            return remoteSphere;
        }
        
        // It should be set to inactive automatically in Bullet.Awake
        GameObject playerSphereClone = GameObject.Instantiate(stasisSpherePrefab);
        playerSphereClone.name = $"remote-{playerId}-{playerSphereClone.name}";
        // We mark it to be able to ignore events from remote bullets
        playerSphereClone.AddComponent<RemotePlayerBullet>();
        StasisSphere stasisSphere = playerSphereClone.GetComponent<StasisSphere>();

        stasisSpherePerPlayerId[playerId] = stasisSphere;
        return stasisSphere;
    }

    private void DestroyPlayerSphere(ushort playerId)
    {
        if (stasisSpherePerPlayerId.TryGetValue(playerId, out StasisSphere stasisSphere) && stasisSphere)
        {
            GameObject.Destroy(stasisSphere.gameObject);
        }
        stasisSpherePerPlayerId.Remove(playerId);
    }

    public IEnumerator Initialize()
    {
        TaskResult<GameObject> result = new();
        
        // Load torpedo types prefab and store them by tech type
        foreach (TechType techType in preloadedVehicleTypes)
        {
            yield return DefaultWorldEntitySpawner.RequestPrefab(techType, result);
            if (result.value && result.value.TryGetComponent(out Vehicle vehicle) && vehicle.torpedoTypes != null)
            {
                foreach (TorpedoType torpedoType in vehicle.torpedoTypes)
                {
                    torpedoPrefabByTechType[torpedoType.techType] = torpedoType.prefab;
                }
            }
        }

        // Load the stasis sphere prefab
        yield return DefaultWorldEntitySpawner.RequestPrefab(TechType.StasisRifle, result);
        StasisRifle rifle = result.value.GetComponent<StasisRifle>();
        if (rifle)
        {
            stasisSpherePrefab = rifle.effectSpherePrefab;
        }

        // Setup remote players' stasis spheres
        foreach (RemotePlayer remotePlayer in playerManager.GetAll())
        {
            EnsurePlayerHasSphere(remotePlayer.PlayerId);
        }

        playerManager.OnCreate += (playerId, _) => { EnsurePlayerHasSphere(playerId); };
        playerManager.OnRemove += (playerId, _) => { DestroyPlayerSphere(playerId); };
    }

    public class RemotePlayerBullet : MonoBehaviour;
}
