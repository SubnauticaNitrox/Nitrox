using NitroxModel.DataStructures;
using System.Collections;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases.PostSpawners;

public interface IEntityPostSpawner
{
    TechType TechType { get; }
    IEnumerator PostSpawnAsync(GameObject gameObject, NitroxId objectId);
}
