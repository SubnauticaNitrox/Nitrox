using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Bases.New;
using NitroxClient.MonoBehaviours;
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
        foreach (InventoryItemEntity childItemEntity in entity.ChildEntities.OfType<InventoryItemEntity>())
        {
            Log.Debug($"Spawning inventory item {childItemEntity.Id}");
            yield return entities.SpawnAsync(childItemEntity);
        }
    }

    public override bool SpawnsOwnChildren(ModuleEntity entity) => true;
}
