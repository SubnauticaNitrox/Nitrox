using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    /*
     * When a Alien Containment Unit is created, multiple objects are spawned: the main world object (WaterParkPiece) and
     * the contained WaterPark as a separate game object (WaterParkPiece, also known as a 'module').  The WaterPark in turn
     * contains a Planter.  When the object spawns, we use this class to set a deterministic id seeded by the parent id. 
     * This keeps inventory actions in sync and allows for persistent storage of each container's contents.
     */
    public class BaseRoomWaterParkProcessor : BasePieceSpawnProcessor
    {
        protected override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseWaterPark
        };

        protected override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece, bool justConstruced)
        {
            NitroxId pieceId = NitroxEntity.GetId(finishedPiece);

            WaterParkPiece waterParkPiece = finishedPiece.GetComponent<WaterParkPiece>();
            if (!waterParkPiece)
            {
                // The BaseWater has multiple base pieces, but only one of them (the bottom) contains the WaterParkPiece component...
                return;
            }

            WaterPark waterPark = waterParkPiece.GetWaterParkModule();
            Validate.NotNull(waterPark, "WaterParkPiece without WaterParkModule?!?");

            // assuming there could be multiple pieces sharing the same waterpark we only create an ID if there is none.
            NitroxEntity waterEntity = waterPark.gameObject.GetComponent<NitroxEntity>();
            if (!waterEntity)
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
