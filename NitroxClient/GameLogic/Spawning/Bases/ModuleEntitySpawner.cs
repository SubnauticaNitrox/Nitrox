using System.Collections;
using NitroxClient.GameLogic.Bases.New;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class ModuleEntitySpawner : EntitySpawner<ModuleEntity>
{
    public override IEnumerator SpawnAsync(ModuleEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Log.Debug($"Spawning a ModuleEntity: {entity.Id}");
        if (NitroxEntity.TryGetObjectFrom(entity.Id, out GameObject gameObject))
        {
            Log.Debug($"Resynced ModuleEntity {entity.Id}");
            GameObject.Destroy(gameObject);
            yield return null;
        }
        yield return NitroxBuild.RestoreModule(LargeWorldStreamer.main.globalRoot.transform, entity.SavedModule, result);
    }

    public override bool SpawnsOwnChildren(ModuleEntity entity) => true;
}
