using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class CyclopsFireEntitySpawner : EntitySpawner<CyclopsFireEntity>
{
    public override IEnumerator SpawnAsync(CyclopsFireEntity entity, TaskResult<Optional<GameObject>> result)
    {
        Optional<GameObject> parentSub = NitroxEntity.GetObjectFrom(entity.ParentId);

        if (!parentSub.HasValue)
        {
            Log.Error($"Could not find cyclops gameobject for fire {entity}");
            result.Set(Optional.Empty);
            yield break;
        }

        SubRoot subRoot = parentSub.Value.GetComponent<SubRoot>();

        if (!subRoot)
        {
            Log.Error($"Could not find subroot for fire on gameobject {parentSub.Value.name}");
            result.Set(Optional.Empty);
            yield break;
        }

        SubFire subFire = subRoot.damageManager.subFire;
        yield return SpawnFire(subFire, (CyclopsRooms)entity.RoomIndex, entity.NodeIndex, entity.Id, result);
    }

    private IEnumerator SpawnFire(SubFire subFire, CyclopsRooms room, int nodeIndex, NitroxId id, TaskResult<Optional<GameObject>> result)
    {
        SubFire.RoomFire roomFire = subFire.roomFires[room];
        Transform fireTransform = roomFire.spawnNodes[nodeIndex];

        // If a fire already exists at the node, replace the old Id with the new one
        if (fireTransform.childCount > 0)
        {
            Fire existingFire = fireTransform.GetComponentInChildren<Fire>();

            if (existingFire && NitroxEntity.GetId(existingFire.gameObject) != id)
            {
                Log.Error($"Fire already exists at node index {nodeIndex}! Updating id to {id}");
                NitroxEntity.SetNewId(existingFire.gameObject, id);

                result.Set(Optional.Of(existingFire.gameObject));
                yield break;
            }
        }

        roomFire.fireValue++;

        PrefabSpawn firePrefab = fireTransform.RequireComponent<PrefabSpawn>();

        GameObject newFireGameObject = null;

        firePrefab.SpawnManual(delegate (GameObject fireGO)
        {
            newFireGameObject = fireGO;
        });

        yield return new WaitUntil(() => newFireGameObject);

        Fire fire = newFireGameObject.GetComponentInChildren<Fire>();

        if (fire)
        {
            fire.fireSubRoot = subFire.subRoot;
            NitroxEntity.SetNewId(fire.gameObject, id);
            result.Set(Optional.Of(fire.gameObject));
        }
        else
        {
            Log.Error($"Fire prefab contained no fire object at node index {nodeIndex} for {id}");
            result.Set(Optional.Empty);
        }
    }
 
    public override bool SpawnsOwnChildren(CyclopsFireEntity entity)
    {
        return false;
    }
}
