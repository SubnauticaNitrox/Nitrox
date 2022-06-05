using System.Collections;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
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

        protected override bool ShouldRerunSpawnProcessor => true;

        protected override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            NitroxId reactorId = NitroxEntity.GetId(finishedPiece);
            BaseBioReactorGeometry bioReactor = finishedPiece.RequireComponent<BaseBioReactorGeometry>();
            BaseBioReactor bioReactorModule = bioReactor.GetModule();
            // When reruning the spawn processor, the module will not be found at first so we need to delay its detection
            if (!bioReactorModule)
            {
                latestBase.StartCoroutine(DelayModuleDetection(latestBase, latestCell, finishedPiece));
                return;
            }

            NitroxId moduleId = reactorId.Increment();
            NitroxEntity.SetNewId(bioReactorModule.gameObject, moduleId);
        }

        private IEnumerator DelayModuleDetection(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            if (!finishedPiece)
            {
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
            SpawnPostProcess(latestBase, latestCell, finishedPiece);
        }
    }
}
