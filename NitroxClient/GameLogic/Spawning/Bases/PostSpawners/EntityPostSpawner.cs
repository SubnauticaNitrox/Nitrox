using NitroxModel.Core;
using NitroxModel.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases.PostSpawners;

public class EntityPostSpawner
{
    private static readonly Dictionary<TechType, IConstructablePostSpawner> constructablePostSpawners;
    private static readonly Dictionary<Type, IBaseModulePostSpawner> modulePostSpawners;

    static EntityPostSpawner()
    {
        constructablePostSpawners = NitroxServiceLocator.LocateService<IEnumerable<IConstructablePostSpawner>>()
                                               .ToDictionary(p => p.TechType);
        modulePostSpawners = NitroxServiceLocator.LocateService<IEnumerable<IBaseModulePostSpawner>>()
                                               .ToDictionary(p => p.GetType().BaseType.GetGenericArguments()[0]);
    }

    public static IEnumerator ApplyPostSpawner(GameObject gameObject, NitroxId objectId)
    {
        if (gameObject.TryGetComponent(out IBaseModule baseModule) &&
            modulePostSpawners.TryGetValue(baseModule.GetType(), out IBaseModulePostSpawner baseModulePostSpawner))
        {
            yield return baseModulePostSpawner.PostSpawnAsync(gameObject, baseModule, objectId);
        }
        else if (gameObject.TryGetComponent(out Constructable constructable) &&
            constructablePostSpawners.TryGetValue(constructable.techType, out IConstructablePostSpawner postSpawner))
        {
            yield return postSpawner.PostSpawnAsync(gameObject, objectId);
        }
    }
}
