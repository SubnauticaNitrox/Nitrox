using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class BenchChangedProcessor : ClientPacketProcessor<BenchChanged>
{
    private readonly PlayerManager remotePlayerManager;

    public BenchChangedProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(BenchChanged benchChanged)
    {
        if (!remotePlayerManager.TryFind(benchChanged.PlayerId, out RemotePlayer remotePlayer))
        {
            Log.Error($"Couldn't find {nameof(RemotePlayer)} for {benchChanged.PlayerId}");
            return;
        }

        if (!NitroxEntity.TryGetObjectFrom(benchChanged.BenchId, out GameObject benchObject))
        {
            Log.Error($"Couldn't find GameObject for {benchChanged.BenchId}");
            return;
        }

        Bench bench = benchObject.GetComponent<Bench>();
        if (!bench)
        {
            bench = benchObject.GetComponentInParent<Bench>();
        }

        if (!bench)
        {
            Log.Error($"Couldn't find {nameof(Bench)} component on {benchChanged.BenchId}");
            return;
        }

        // Determine if this is a chair or bench based on the cinematic controller's animation name
        bool isChair = bench.cinematicController && bench.cinematicController.playerViewAnimationName.Contains("chair");
        string sitAnimation = isChair ? "chair_sit" : "bench_sit";
        string standAnimation = isChair ? "chair_stand_up" : "bench_stand_up";

        bool isCinematicActive = benchChanged.ChangeState != BenchChanged.BenchChangeState.UNSET;

        // Set InCinematic flag to prevent movement packets from overriding animation state
        remotePlayer.InCinematic = isCinematicActive;
        // Disable velocity-based animations during cinematics to prevent interference
        remotePlayer.AnimationController.UpdatePlayerAnimations = !isCinematicActive;
        remotePlayer.AnimationController["cinematics_enabled"] = isCinematicActive;
        remotePlayer.AnimationController[sitAnimation] = benchChanged.ChangeState == BenchChanged.BenchChangeState.SITTING_DOWN;
        remotePlayer.AnimationController[standAnimation] = benchChanged.ChangeState == BenchChanged.BenchChangeState.STANDING_UP;

        GameObject benchParent;
        if (benchObject.GetComponent<Constructable>())
        {
            benchParent = benchObject;
        }
        else if (benchObject.transform.parent && benchObject.transform.parent.GetComponent<Constructable>())
        {
            benchParent = benchObject.transform.parent.gameObject;
        }
        else
        {
            Log.Error($"Couldn't find {nameof(Constructable)} component on {benchChanged.BenchId} or its parent");
            return;
        }

        RemotePlayerBenchBlocker benchBlocker = benchParent.EnsureComponent<RemotePlayerBenchBlocker>();

        switch (benchChanged.ChangeState)
        {
            case BenchChanged.BenchChangeState.SITTING_DOWN:
                benchBlocker.AddPlayerToBench(remotePlayer);
                PositionPlayerOnSeat(remotePlayer, benchObject, benchParent, bench, isChair);
                break;
            case BenchChanged.BenchChangeState.UNSET:
                benchBlocker.RemovePlayerFromBench(remotePlayer);
                break;
        }
    }

    private static void PositionPlayerOnSeat(RemotePlayer remotePlayer, GameObject seatObject, GameObject benchParent, Bench bench, bool isChair)
    {
        if (isChair)
        {
            if (bench.cinematicController && bench.cinematicController.animatedTransform)
            {
                remotePlayer.Body.transform.position = bench.cinematicController.animatedTransform.position;
                remotePlayer.Body.transform.rotation = bench.cinematicController.animatedTransform.rotation;
            }
            return;
        }

        // Benches use the bench_animation hierarchy for positioning
        Transform benchTransform = benchParent.transform;
        Transform animationRoot = benchTransform.Find("bench_animation");
        if (!animationRoot)
        {
            return;
        }

        Transform playerTarget = animationRoot.Find("root/cine_loc/player_target");
        if (!playerTarget)
        {
            return;
        }

        // Position the remote player at the seat location
        // Seat offset (x) determines left/center/right position, playerTarget provides y/z offset and rotation
        Vector3 seatOffset = seatObject.transform.localPosition;
        Vector3 seatWorldPosition = benchTransform.TransformPoint(seatOffset + new Vector3(0, playerTarget.localPosition.y, playerTarget.localPosition.z));
        Quaternion seatRotation = benchTransform.rotation * playerTarget.localRotation;

        remotePlayer.Body.transform.position = seatWorldPosition;
        remotePlayer.Body.transform.rotation = seatRotation;
    }
}
