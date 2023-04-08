using System.Collections;
using NitroxClient.GameLogic.Bases.New;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class GhostEntitySpawner : EntitySpawner<GhostEntity>
{
    public override IEnumerator SpawnAsync(GhostEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Log.Debug($"Spawning a GhostEntity: {entity.Id}");
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject))
        {
            if (gameObject.TryGetComponent(out Constructable constructable))
            {
                constructable.constructedAmount = 0;
                yield return constructable.ProgressDeconstruction();
            }
            Log.Debug($"Resynced GhostEntity {entity.Id}");
            GameObject.Destroy(gameObject);
            yield return null;
        }

        yield return NitroxBuild.RestoreGhost(LargeWorldStreamer.main.globalRoot.transform, entity, result);
    }

    public override bool SpawnsOwnChildren(GhostEntity entity) => true;
}
