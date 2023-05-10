using NitroxModel.DataStructures;
using System.Collections;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases.PostSpawners;

public interface IBaseModulePostSpawner
{
    IEnumerator PostSpawnAsync(GameObject gameObject, IBaseModule baseModule, NitroxId objectId);
}

public abstract class BaseModulePostSpawner<T> : IBaseModulePostSpawner where T : IBaseModule
{
    public abstract IEnumerator PostSpawnAsync(GameObject gameObject, T baseModule, NitroxId objectId);

    public IEnumerator PostSpawnAsync(GameObject gameObject, IBaseModule baseModule, NitroxId objectId)
    {
        yield return PostSpawnAsync(gameObject, (T)baseModule, objectId);
    }
}
