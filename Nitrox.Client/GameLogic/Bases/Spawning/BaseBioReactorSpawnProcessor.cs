using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using UnityEngine;

namespace Nitrox.Client.GameLogic.Bases.Spawning
{
    /*
     * When a bio reactor is created, two objects are spawned: the main world object (BaseBioReactorGeometry) and
     * the core power logic as a separate game object (BaseBioReactor, also known as a 'module').  The BaseBioReactor 
     * resides as a direct child of the base object (probably so UWE could iterate them easy).  When the object spawns, 
     * we use this class to set a deterministic id seeded by the parent id.  This keeps inventory actions in sync.
     */
    public class BaseBioReactorSpawnProcessor : BasePieceSpawnProcessor
    {
        public override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseBioReactor
        };

        public override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            NitroxId reactorId = NitroxEntity.GetId(finishedPiece);
            BaseBioReactorGeometry bioReactor = finishedPiece.RequireComponent<BaseBioReactorGeometry>();
            GameObject bioReactorModule = ((BaseBioReactor)bioReactor.ReflectionCall("GetModule")).gameObject;

            NitroxId moduleId = reactorId.Increment();
            NitroxEntity.SetNewId(bioReactorModule, moduleId);
        }

    }
}
