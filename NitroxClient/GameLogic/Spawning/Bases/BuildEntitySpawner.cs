using System;
using System.Collections;
using System.Linq;
using NitroxClient.GameLogic.Bases.EntityUtils;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class BuildEntitySpawner : EntitySpawner<BuildEntity>
{
    private readonly Entities entities;

    public BuildEntitySpawner(Entities entities)
    {
        this.entities = entities;
    }

    public override IEnumerator SpawnAsync(BuildEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Log.Debug($"Spawning a BuildEntity: {entity.Id}");
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject) && gameObject)
        {
            Log.Error("Trying to respawn an already spawned Base without a proper resync process.");
            yield break;
        }

        DateTimeOffset beginTime = DateTimeOffset.Now;
        yield return NitroxBuild.CreateBuild(entity, result);
        DateTimeOffset endTime = DateTimeOffset.Now;
        Log.Debug(string.Format("Took {0}ms to create the Base", (endTime - beginTime).TotalMilliseconds));

        yield return entities.SpawnAsync(entity.ChildEntities.OfType<PlayerWorldEntity>());
        if (result.value.HasValue && result.value.Value.TryGetComponent(out Base @base))
        {
            yield return NitroxBuild.RestoreMoonpools(entity.ChildEntities.OfType<MoonpoolEntity>(), @base);
            yield return NitroxBuild.RestoreMapRooms(entity.ChildEntities.OfType<MapRoomEntity>(), @base);
        }
    }

    public override bool SpawnsOwnChildren(BuildEntity entity) => true;
}
