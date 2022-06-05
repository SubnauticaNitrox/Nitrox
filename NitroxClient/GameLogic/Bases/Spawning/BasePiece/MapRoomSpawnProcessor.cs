using System;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    /*
     * When a map room is created, two objects are added to the base: a BaseMapRoom piece that is
     * entirely geometry and a MapRoomFunctionality game object in the base root.  This class sets
     * deterministic ids on non-geometry pieces, such as the map module upgrade storage area, so 
     * they can stay in sync during player interactions.
     */
    public class MapRoomSpawnProcessor : BasePieceSpawnProcessor
    {
        protected override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseMapRoom
        };

        protected override bool ShouldRerunSpawnProcessor => false;

        protected override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            NitroxId mapRoomGeometryPieceId = NitroxEntity.GetId(finishedPiece);
            GameObject mapRoomFunctionality = FindUntaggedMapRoomFunctionality(latestBase);

            NitroxId mapRoomFunctionalityId = mapRoomGeometryPieceId.Increment();
            NitroxEntity.SetNewId(mapRoomFunctionality, mapRoomFunctionalityId);

            GameObject mapRoomModules = mapRoomFunctionality.FindChild("MapRoomUpgrades");
            NitroxId mapRoomModulesId = mapRoomFunctionalityId.Increment();
            NitroxEntity.SetNewId(mapRoomModules, mapRoomModulesId);
        }

        private static GameObject FindUntaggedMapRoomFunctionality(Base latestBase)
        {
            foreach (Transform child in latestBase.transform)
            {
                if (child.GetComponent<MapRoomFunctionality>() && !child.GetComponent<NitroxEntity>())
                {
                    return child.gameObject;
                }
            }
            throw new ArgumentException($"Unable to locate recently built map room with {latestBase}");
        }
    }
}
