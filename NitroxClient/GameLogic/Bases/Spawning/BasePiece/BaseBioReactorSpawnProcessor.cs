using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    /*
     * When a bio reactor is created, two objects are spawned: the main world object (BaseBioReactorGeometry) and
     * the core power logic as a separate game object (BaseBioReactor, also known as a 'module').  The BaseBioReactor 
     * resides as a direct child of the base object (probably so UWE could iterate them easy).  When the object spawns, 
     * we use this class to set a deterministic id seeded by the parent id.  This keeps inventory actions in sync.
     */
    public class BaseBioReactorSpawnProcessor : BasePieceSpawnProcessor
    {
        protected override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseBioReactor
        };

        protected override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece, bool justConstructed)
        {
            NitroxId reactorId = NitroxEntity.GetId(finishedPiece);
            BaseBioReactorGeometry bioReactor = finishedPiece.RequireComponent<BaseBioReactorGeometry>();
            GameObject bioReactorModule = bioReactor.GetModule().gameObject;

            NitroxId moduleId = reactorId.Increment();
            NitroxEntity.SetNewId(bioReactorModule, moduleId);
        }

    }
}
