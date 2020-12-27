using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;
using NitroxModel.Logger;
using NitroxModel.Helper;
using NitroxClient.GameLogic.Helper;

namespace NitroxClient.GameLogic.Bases.Spawning
{
    /*
     * When a bio reactor is created, two objects are spawned: the main world object (BaseBioReactorGeometry) and
     * the core power logic as a separate game object (BaseBioReactor, also known as a 'module').  The BaseBioReactor 
     * resides as a direct child of the base object (probably so UWE could iterate them easy).  When the object spawns, 
     * we use this class to set a deterministic id seeded by the parent id.  This keeps inventory actions in sync.
     */
    public class BaseRoomWaterParkProcessor: BasePieceSpawnProcessor
    {
        public override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseWaterPark
        };

        public override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece)
        {
            NitroxId pieceId = NitroxEntity.GetId(finishedPiece);

            WaterParkPiece waterParkPiece = finishedPiece.GetComponent<WaterParkPiece>();
            if(null==waterParkPiece)
            {
                // The BaseWater has multiple base pieces, but only one of them (the bottom) contains the WaterParkPiece component...
                return;
            }

            WaterPark waterPark = waterParkPiece.GetWaterParkModule();
            Validate.NotNull(waterPark, "WaterParkPiece without WaterParkModule?!?");

            // assuming there could be multiple pieces sharing the same waterpark we only create an ID if there is none.
            NitroxEntity waterEntity = waterPark.gameObject.GetComponent<NitroxEntity>();
            if (null == waterEntity)
            {
                NitroxId newWaterparkId = pieceId.Increment();
                NitroxEntity.SetNewId(waterPark.gameObject, newWaterparkId);

                NitroxId newPlanterId = newWaterparkId.Increment();
                NitroxEntity.SetNewId(waterPark.planter.gameObject, newPlanterId);

                Log.Debug($"BaseRoomWaterParkProcessor: Created new Waterpark {newWaterparkId} and Planter {newPlanterId}");
            }
        }
    }
}
