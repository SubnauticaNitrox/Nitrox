using System.Collections;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class CreatureRespawnEntitySpawner : IWorldEntitySpawner, IWorldEntitySyncSpawner
{
    private readonly SimulationOwnership simulationOwnership;

    public CreatureRespawnEntitySpawner(SimulationOwnership simulationOwnership)
    {
        this.simulationOwnership = simulationOwnership;
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not CreatureRespawnEntity creatureRespawnEntity)
        {
            yield break;
        }

        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(entity.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for {nameof(OxygenPipeEntity)} of ClassId {entity.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(creatureRespawnEntity, gameObject, out Respawn respawn))
        {
            yield break;
        }

        SetupObject(creatureRespawnEntity, gameObject, respawn);

        result.Set(gameObject);
    }

    public bool SpawnSync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        if (entity is not CreatureRespawnEntity creatureRespawnEntity)
        {
            return true;
        }

        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: entity.ClassId))
        {
            return false;
        }

        GameObject gameObject = GameObjectHelper.InstantiateInactiveWithId(prefab, entity.Id);
        if (!VerifyCanSpawnOrError(creatureRespawnEntity, gameObject, out Respawn respawn))
        {
            return true;
        }

        SetupObject(creatureRespawnEntity, gameObject, respawn);

        result.Set(gameObject);
        return true;
    }

    public bool SpawnsOwnChildren() => false;

    private bool VerifyCanSpawnOrError(CreatureRespawnEntity entity, GameObject gameObject, out Respawn respawn)
    {
        // Respawn's logic only work during their Start method so we'll either execute it directly when spawning this entity, or destroy it
        if (DayNightCycle.main.timePassedAsFloat < entity.SpawnTime)
        {
            GameObject.Destroy(gameObject);
            respawn = null;
            return false;
        }
        if (gameObject.TryGetComponent(out respawn))
        {
            return true;
        }

        Log.Error($"Could not find component {nameof(Respawn)} on prefab with ClassId: {entity.ClassId}");
        return false;
    }

    private void SetupObject(CreatureRespawnEntity entity, GameObject gameObject, Respawn respawn)
    {
        RespawnContext context = new() { Entity = entity, GameObject = gameObject, Respawn = respawn };
        LockRequest<RespawnContext> lockRequest = new(entity.Id, SimulationLockType.TRANSIENT, TriggerRespawnCallback, context);

        simulationOwnership.RequestSimulationLock(lockRequest);
    }

    private static void TriggerRespawnCallback(NitroxId entityId, bool acquired, RespawnContext context)
    {
        if (!acquired)
        {
            GameObject.Destroy(context.GameObject);
            return;
        }

        GameObject gameObject = context.GameObject;
        CreatureRespawnEntity entity = context.Entity;
        Respawn respawn = context.Respawn;

        // This will only happen if the respawn is ready to be activated
        Transform transform = gameObject.transform;
        transform.localPosition = entity.Transform.Position.ToUnity();
        transform.localRotation = entity.Transform.Rotation.ToUnity();
        transform.localScale = entity.Transform.LocalScale.ToUnity();

        // It's possible that either the respawn was parented to something (e.g. a Reefback) or directly to a cell
        if (entity.ParentId != null)
        {
            if (LargeWorldStreamer.main)
            {
                LargeWorldStreamer.main.cellManager.UnregisterEntity(gameObject);
            }
            if (NitroxEntity.TryGetComponentFrom(entity.ParentId, out Transform parent))
            {
                transform.parent = parent;
            }
        }
        else if (gameObject.TryGetComponent(out LargeWorldEntity largeWorldEntity))
        {
            largeWorldEntity.cellLevel = (LargeWorldEntity.CellLevel)entity.Level;
            if (LargeWorldStreamer.main)
            {
                LargeWorldStreamer.main.cellManager.RegisterEntity(gameObject);
            }
        }

        respawn.spawnTime = entity.SpawnTime;
        respawn.techType = entity.RespawnTechType.ToUnity();
        respawn.addComponents.Clear();
        respawn.addComponents.AddRange(entity.AddComponents);

        gameObject.SetActive(true);
    }

    internal class RespawnContext : LockRequestContext
    {
        public CreatureRespawnEntity Entity;
        public GameObject GameObject;
        public Respawn Respawn;
    }
}
