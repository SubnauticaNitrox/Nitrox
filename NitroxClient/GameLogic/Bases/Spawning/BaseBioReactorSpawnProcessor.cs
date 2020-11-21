using System.Linq;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning
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

            NitroxId moduleId = generateDeterministicReactorModuleId(reactorId);
            NitroxEntity.SetNewId(bioReactorModule, moduleId);
        }

        private NitroxId generateDeterministicReactorModuleId(NitroxId parentId)
        {
            string guid = parentId.ToString();
            char newLastChar = (guid.Last() == '0') ? '1' : '0';

            string moduleGuid = guid.Substring(0, guid.Length - 1) + newLastChar;

            return new NitroxId(moduleGuid);
        }

    }
}
