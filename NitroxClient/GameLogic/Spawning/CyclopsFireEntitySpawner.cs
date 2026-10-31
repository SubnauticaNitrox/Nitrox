using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class CyclopsFireEntitySpawner : SyncEntitySpawner<CyclopsFireEntity>
{
    protected override IEnumerator SpawnAsync(CyclopsFireEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        return null;
    }

    protected override bool SpawnSync(CyclopsFireEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!NitroxEntity.TryGetComponentFrom(entity.ParentId, out SubRoot subRoot))
        {
            Log.Error($"[{nameof(CyclopsFireEntitySpawner)}] Couldn't find a {nameof(SubRoot)} from id {entity.ParentId}");
            result.Set(Optional.Empty);
            return true;
        }

        SubFire subFire = subRoot.damageManager.subFire;
        SubFire.RoomFire roomFire = subFire.roomFires[entity.Room];
        Transform spawnNode = roomFire.spawnNodes[entity.NodeIndex];

        // If a fire already exists at the node, replace the old Id with the new one
        if (spawnNode.childCount > 0)
        {
            Transform existingFire = spawnNode.RequireComponentInChildren<Fire>().transform.parent;
            NitroxEntity.SetNewId(existingFire.gameObject, entity.Id);

            result.Set(existingFire.gameObject);
            return true;
        }

        roomFire.fireValue++;

        // The callback always runs synchronously
        spawnNode.RequireComponent<PrefabSpawn>().SpawnManual(gameObject =>
        {
            Fire fire = gameObject.RequireComponentInChildren<Fire>();
            fire.fireSubRoot = subRoot;
            NitroxEntity.SetNewId(fire.transform.parent.gameObject, entity.Id);

            result.Set(fire.gameObject);
        });

        return true;
    }

    protected override bool SpawnsOwnChildren(CyclopsFireEntity entity) => false;
}
