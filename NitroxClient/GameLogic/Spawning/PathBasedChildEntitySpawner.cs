using System.Collections;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using static NitroxModel.DisplayStatusCodes;

namespace NitroxClient.GameLogic.Spawning;

public class PathBasedChildEntitySpawner : SyncEntitySpawner<PathBasedChildEntity>
{
    protected override IEnumerator SpawnAsync(PathBasedChildEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        yield break;
    }

    protected override bool SpawnSync(PathBasedChildEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Optional<GameObject> owner = NitroxEntity.GetObjectFrom(entity.ParentId);

        if (!owner.HasValue)
        {
            DisplayStatusCode(StatusCode.SUBNAUTICA_ERROR, $"Unable to find parent entity: {entity}");
            result.Set(Optional.Empty);
            return true;
        }

        Transform child = owner.Value.transform.Find(entity.Path);

        if (!child)
        {
            DisplayStatusCode(StatusCode.SUBNAUTICA_ERROR, $"Could not locate child at path {entity.Path} in {owner.Value.name}");
            result.Set(Optional.Empty);
            return true;
        }

        GameObject gameObject = child.gameObject;
        NitroxEntity.SetNewId(gameObject, entity.Id);

        result.Set(gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(PathBasedChildEntity entity) => false;
}
