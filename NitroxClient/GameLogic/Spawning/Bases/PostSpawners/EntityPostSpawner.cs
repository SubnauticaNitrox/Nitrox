using NitroxModel.Core;
using NitroxModel.DataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases.PostSpawners;

public class EntityPostSpawner
{
    private static readonly Dictionary<TechType, IEntityPostSpawner> postSpawners;

    static EntityPostSpawner()
    {
        postSpawners = NitroxServiceLocator.LocateService<IEnumerable<IEntityPostSpawner>>()
                                               .ToDictionary(p => p.TechType);
    }

    public static IEnumerator ApplyPostSpawner(GameObject gameObject, NitroxId objectId)
    {
        if (gameObject.TryGetComponent(out Constructable constructable) &&
            postSpawners.TryGetValue(constructable.techType, out IEntityPostSpawner postSpawner))
        {
            yield return postSpawner.PostSpawnAsync(gameObject, objectId);
        }
    }
}
