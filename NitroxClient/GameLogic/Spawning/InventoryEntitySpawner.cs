using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class InventoryEntitySpawner : SyncEntitySpawner<InventoryEntity>
{
    protected override IEnumerator SpawnAsync(InventoryEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        yield break;
    }

    protected override bool SpawnSync(InventoryEntity entity, TaskResult<Optional<GameObject>> result)
    {
        GameObject parent = NitroxEntity.RequireObjectFrom(entity.ParentId);
        StorageContainer container = parent.GetAllComponentsInChildren<StorageContainer>()
                                            .ElementAt(entity.ComponentIndex);

        if (container)
        {
            NitroxEntity.SetNewId(container.gameObject, entity.Id);
            result.Set(Optional.OfNullable(container.gameObject));
        }
        else
        {
            Log.Error($"Unable to find {nameof(StorageContainer)} for: {entity}");
            result.Set(Optional.Empty);
        }
        return true;
    }

    protected override bool SpawnsOwnChildren(InventoryEntity entity) => false;
}
