using System.Collections;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class PathBasedChildEntitySpawner : EntitySpawner<PathBasedChildEntity>
{
    protected override IEnumerator SpawnAsync(PathBasedChildEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Optional<GameObject> owner = NitroxEntity.GetObjectFrom(entity.ParentId);

        if (!owner.HasValue)
        {
            Log.Error($"Unable to find parent entity: {entity}");
            result.Set(Optional.Empty);
            yield break;
        }

        Transform child = owner.Value.transform.Find(entity.Path);

        if (!child)
        {
            Log.Error($"Could not locate child at path {entity.Path} in {owner.Value.name}");
            result.Set(Optional.Empty);
            yield break;
        }

        GameObject gameObject = child.gameObject;
        NitroxEntity.SetNewId(gameObject, entity.Id);

        result.Set(gameObject);
    }

    protected override bool SpawnsOwnChildren(PathBasedChildEntity entity) => false;
}
