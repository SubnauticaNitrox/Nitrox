using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.Bases.New;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class ModuleEntitySpawner : EntitySpawner<ModuleEntity>
{
    private readonly Entities entities;

    public ModuleEntitySpawner(Entities entities)
    {
        this.entities = entities;
    }

    public override IEnumerator SpawnAsync(ModuleEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Log.Debug($"Spawning a ModuleEntity: {entity.Id}");
        
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject) && gameObject)
        {
            Log.Error("Trying to respawn an already spawned module without a proper resync process.");
            yield break;
        }
        Transform parent = BuildingTester.GetParentOrGlobalRoot(entity.ParentId);

        yield return NitroxBuild.RestoreModule(parent, entity, result);

        if (!result.Get().HasValue)
        {
            Log.Error($"Module couldn't be spawned {entity}");
            yield break;
        }
        GameObject moduleObject = result.Get().Value;
        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(moduleObject);
        if (!opContainer.HasValue)
        {
            yield break;
        }

        DateTimeOffset beginTime = DateTimeOffset.Now;
        List<Entity> rootEntitiesToSpawn = entity.ChildEntities.OfType<InventoryItemEntity>().ToList<Entity>();

        yield return entities.SpawnBatchAsync(rootEntitiesToSpawn, true);

        DateTimeOffset endTime = DateTimeOffset.Now;
        Log.Debug($"Module complete spawning took {(endTime - beginTime).TotalMilliseconds}ms");

        if (moduleObject.TryGetComponent(out PowerSource powerSource))
        {
            NitroxBuild.SetupPower(powerSource);
        }
    }

    public override bool SpawnsOwnChildren(ModuleEntity entity) => true;
}
