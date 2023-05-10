using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using System;
using System.Collections;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases.PostSpawners;

/// <summary>
/// For better immersion we split the Bench in three parts (left/center/right). On each can sit one player.
/// </summary>
public class BenchPostSpawner : IConstructablePostSpawner
{
    public TechType TechType => TechType.Bench;
    private const int LAYER_USEABLE = 13;

    public IEnumerator PostSpawnAsync(GameObject gameObject, NitroxId benchId)
    {
        Log.Debug($"BenchPostSpawner.PostSpawnAsync({benchId})");
        if (gameObject.TryGetComponent(out Bench bench))
        {
            try
            {
                GameObject benchTileLeft = new("BenchPlaceLeft") { layer = LAYER_USEABLE };
                benchTileLeft.transform.SetParent(gameObject.transform, false);
                benchTileLeft.transform.localPosition -= new Vector3(0.75f, 0, 0);
                BoxCollider benchTileLeftCollider = benchTileLeft.AddComponent<BoxCollider>();
                benchTileLeftCollider.center = new Vector3(0, 0.25f, 0);
                benchTileLeftCollider.size = new Vector3(0.85f, 0.5f, 0.65f);
                benchTileLeftCollider.isTrigger = true;

                GameObject benchTileCenter = new("BenchPlaceCenter") { layer = LAYER_USEABLE };
                benchTileCenter.transform.SetParent(gameObject.transform, false);
                BoxCollider benchTileCenterCollider = benchTileCenter.AddComponent<BoxCollider>();
                benchTileCenterCollider.center = new Vector3(0, 0.25f, 0);
                benchTileCenterCollider.size = new Vector3(0.7f, 0.5f, 0.65f);
                benchTileCenterCollider.isTrigger = true;

                GameObject benchTileRight = new("BenchPlaceRight") { layer = LAYER_USEABLE };
                benchTileRight.transform.SetParent(gameObject.transform, false);
                benchTileRight.transform.localPosition += new Vector3(0.75f, 0, 0);
                BoxCollider benchTileRightCollider = benchTileRight.AddComponent<BoxCollider>();
                benchTileRightCollider.center = new Vector3(0, 0.25f, 0);
                benchTileRightCollider.size = new Vector3(0.85f, 0.5f, 0.65f);
                benchTileRightCollider.isTrigger = true;

                GameObject animationRoot = gameObject.FindChild("bench_animation");

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
                UnityEngine.Object.Destroy(gameObject.FindChild("Builder Trigger"));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        yield break;
    }
}
