using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class CyclopsDamagePointEntitySpawner : SyncEntitySpawner<CyclopsDamagePointEntity>
{
    protected override IEnumerator SpawnAsync(CyclopsDamagePointEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        return null;
    }

    protected override bool SpawnSync(CyclopsDamagePointEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!NitroxEntity.TryGetComponentFrom(entity.ParentId, out SubRoot subRoot))
        {
            Log.Error($"[{nameof(CyclopsFireEntitySpawner)}] Couldn't find a {nameof(SubRoot)} from id {entity.ParentId}");
            result.Set(Optional.Empty);
            return true;
        }

        CyclopsExternalDamageManager damageManager = subRoot.damageManager;
        CyclopsDamagePoint damagePoint = damageManager.damagePoints[entity.DamagePointIndex];

        if (!damagePoint.gameObject.activeSelf)
        {
            // Copied from CyclopsExternalDamageManager.CreatePoint(), except without the random index pick.
            damagePoint.gameObject.SetActive(true);
            damagePoint.RestoreHealth();
            GameObject prefabGo = damageManager.fxPrefabs[Random.Range(0, damageManager.fxPrefabs.Length)];
            damagePoint.SpawnFx(prefabGo);
            damageManager.unusedDamagePoints.Remove(damagePoint);
        }

        NitroxEntity.SetNewId(damagePoint.gameObject, entity.Id);
        result.Set(damagePoint.gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(CyclopsDamagePointEntity entity) => false;
}
