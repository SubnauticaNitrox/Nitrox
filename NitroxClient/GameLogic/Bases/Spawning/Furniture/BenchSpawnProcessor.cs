using System;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.Furniture;

// For better immersion we split the Bench in three parts (left/center/right). One player can sit on each part.
public class BenchSpawnProcessor : FurnitureSpawnProcessor
{
    private const int LAYER_USEABLE = 13;

    protected override TechType[] ApplicableTechTypes { get; } =
    {
        TechType.Bench
    };

    protected override void SpawnPostProcess(GameObject finishedFurniture)
    {
        GameObject CreateBenchTile(string name, Vector3 offset)
        {
            GameObject benchTile = new GameObject(name) { layer = LAYER_USEABLE };
            benchTile.transform.SetParent(finishedFurniture.transform, false);
            benchTile.transform.localPosition += offset;
            BoxCollider benchTileCollider = benchTile.AddComponent<BoxCollider>();
            benchTileCollider.center = new Vector3(0, 0.25f, 0);
            benchTileCollider.size = new Vector3(0.85f, 0.5f, 0.65f);
            benchTileCollider.isTrigger = true;
            return benchTile;
        }

        if (!finishedFurniture.TryGetComponent(out Bench bench))
        {
            return;
        }

        try
        {
            if (!finishedFurniture.TryGetIdOrWarn(out NitroxId benchId))
            {
                throw new InvalidOperationException("Couldn't retrieve id from Bench");
            }

            GameObject benchTileLeft = CreateBenchTile("BenchPlaceLeft", new Vector3(-0.75f, 0, 0));
            GameObject benchTileCenter = CreateBenchTile("BenchPlaceCenter", Vector3.zero);
            GameObject benchTileRight = CreateBenchTile("BenchPlaceRight", new Vector3(0.75f, 0, 0));

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
