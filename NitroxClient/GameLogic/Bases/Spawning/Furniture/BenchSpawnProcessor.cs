using System;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.Furniture
{
    // For better immersion we split the Bench in three parts (left/center/right). On each can sit one player.
    public class BenchSpawnProcessor : FurnitureSpawnProcessor
    {
        private const int LAYER_USEABLE = 13;

        protected override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.Bench
        };

        protected override void SpawnPostProcess(GameObject finishedFurniture)
        {
            if (finishedFurniture.TryGetComponent(out Bench bench))
            {
                try
                {
                    NitroxId benchId = NitroxEntity.GetId(finishedFurniture);

                    GameObject benchTileLeft = new GameObject("BenchPlaceLeft") { layer = LAYER_USEABLE };
                    benchTileLeft.transform.SetParent(finishedFurniture.transform, false);
                    benchTileLeft.transform.localPosition -= new Vector3(0.75f, 0, 0);
                    BoxCollider benchTileLeftCollider = benchTileLeft.AddComponent<BoxCollider>();
                    benchTileLeftCollider.center = new Vector3(0, 0.25f, 0);
                    benchTileLeftCollider.size = new Vector3(0.85f, 0.5f, 0.65f);
                    benchTileLeftCollider.isTrigger = true;

                    GameObject benchTileCenter = new GameObject("BenchPlaceCenter") { layer = LAYER_USEABLE };
                    benchTileCenter.transform.SetParent(finishedFurniture.transform, false);
                    BoxCollider benchTileCenterCollider = benchTileCenter.AddComponent<BoxCollider>();
                    benchTileCenterCollider.center = new Vector3(0, 0.25f, 0);
                    benchTileCenterCollider.size = new Vector3(0.7f, 0.5f, 0.65f);
                    benchTileCenterCollider.isTrigger = true;

                    GameObject benchTileRight = new GameObject("BenchPlaceRight") { layer = LAYER_USEABLE };
                    benchTileRight.transform.SetParent(finishedFurniture.transform, false);
                    benchTileRight.transform.localPosition += new Vector3(0.75f, 0, 0);
                    BoxCollider benchTileRightCollider = benchTileRight.AddComponent<BoxCollider>();
                    benchTileRightCollider.center = new Vector3(0, 0.25f, 0);
                    benchTileRightCollider.size = new Vector3(0.85f, 0.5f, 0.65f);
                    benchTileRightCollider.isTrigger = true;

                    GameObject animationRoot = finishedFurniture.FindChild("bench_animation");

                    MultiplayerBench.FromBench(bench, benchTileLeft, MultiplayerBench.Side.LEFT, animationRoot);
                    MultiplayerBench.FromBench(bench, benchTileCenter, MultiplayerBench.Side.CENTER, animationRoot);
                    MultiplayerBench.FromBench(bench, benchTileRight, MultiplayerBench.Side.RIGHT, animationRoot);


                    NitroxId benchLeftId = benchId.Increment();
                    NitroxId benchCenterId = benchLeftId.Increment();
                    NitroxId benchRightId = benchCenterId.Increment();
                    NitroxEntity.SetNewId(benchTileLeft, benchLeftId);
                    NitroxEntity.SetNewId(benchTileCenter, benchCenterId);
                    NitroxEntity.SetNewId(benchTileRight, benchRightId);

                    UnityEngine.Object.Destroy(bench);
                    UnityEngine.Object.Destroy(finishedFurniture.FindChild("Builder Trigger"));
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
    }
}
