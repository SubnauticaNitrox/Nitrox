using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece;

public class BaseMoonpoolSpawnProcessor : BasePieceSpawnProcessor
{
    protected override TechType[] ApplicableTechTypes { get; } =
    {
        TechType.BaseMoonpool
    };

    protected override bool ShouldRerunSpawnProcessor => true;

    protected override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
    {
        if (!finishedPiece.TryGetIdOrWarn(out NitroxId moonpoolId))
        {
            return;
        }

        VehicleDockingBay dockingBay = finishedPiece.RequireComponentInChildren<VehicleDockingBay>();

        NitroxId dockingBayId = moonpoolId.Increment();
        NitroxEntity.SetNewId(dockingBay.gameObject, dockingBayId);
    }
}
