using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using System.Collections;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases.PostSpawners;

public class LargeRoomWaterParkPostSpawner : BaseModulePostSpawner<LargeRoomWaterPark>
{
    public override IEnumerator PostSpawnAsync(GameObject gameObject, LargeRoomWaterPark baseModule, NitroxId objectId)
    {
        NitroxId leftId = objectId.Increment();
        NitroxId rightId = leftId.Increment();

        NitroxEntity.SetNewId(baseModule.planters.leftPlanter.gameObject, leftId);
        NitroxEntity.SetNewId(baseModule.planters.rightPlanter.gameObject, rightId);
        yield break;
    }
}
