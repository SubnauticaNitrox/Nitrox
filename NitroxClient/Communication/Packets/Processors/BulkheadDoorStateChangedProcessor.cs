using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UWE;

public class BulkheadDoorStateChangedProcessor : ClientPacketProcessor<BulkheadDoorStateChanged>
{

    private readonly PlayerManager remotePlayerManager;

    public BulkheadDoorStateChangedProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }


    public override void Process(BulkheadDoorStateChanged packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject bulkheadDoor))
        {
            Log.Error($"[BulkheadDoorStateChangedProcessor] Couldn't find GameObject for {packet.Id}");
            return;
        }

        if (!remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer otherPlayer))
        {
            Log.Error($"[BulkheadDoorStateChangedProcessor] Couldn't find {nameof(RemotePlayer)} for {packet.PlayerId}");
            return;
        }

        if (!bulkheadDoor.TryGetComponentInChildren(out BulkheadDoor door))
        {
            Log.Info("[BulkheadDoorStateChangedProcessor] Unable to find BulkheadDoor");
            return;
        }

        bool prevState = !packet.IsOpen;

        door.animator.SetBool("player_in_front", packet.IsFacingDoor);

        PlayerCinematicController cinematicController = GetPlayerCinematicController(door, prevState, packet.IsFacingDoor);
        MultiplayerCinematicController multiPlayerCinematicController = MultiplayerCinematicController.Initialize(cinematicController);

        multiPlayerCinematicController.CallStartCinematicMode(otherPlayer);
        PlayDoorSound(door, packet.IsOpen);

        CoroutineHost.StartCoroutine(SetDoorStateAfterDelay(door, packet.IsOpen, 3.0f));
    }

    private void PlayDoorSound(BulkheadDoor door, bool isOpen)
    {
        bool isFacingDoor = door.GetSide();
        string soundPath = GetDoorSoundPath(isOpen, isFacingDoor);
        FMODAsset bulkheadOpenFrontAsset = Resources.Load<FMODAsset>(soundPath);
        if (bulkheadOpenFrontAsset != null)
        {
            Utils.PlayFMODAsset(bulkheadOpenFrontAsset, door.transform, 1.0f);
        }
        else
        {
            Log.Error($"[BulkheadDoorStateChangedProcessor] FMODAsset '{soundPath}' not found.");
        }
    }

    private string GetDoorSoundPath(bool isOpen, bool isFacingDoor)
    {
        string soundPath = "event:/sub/base/bulkhead_";

        soundPath += isOpen ? "open" : "close";
        soundPath += "_";
        soundPath += isFacingDoor ? "front" : "back";

        return soundPath;
    }


    private IEnumerator SetDoorStateAfterDelay(BulkheadDoor door, bool isOpen, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (door != null)
        {
            door.SetState(isOpen);
        }
    }

    private PlayerCinematicController GetPlayerCinematicController(BulkheadDoor door, bool isOpened, bool isFacingDoor)
    {
        if (isFacingDoor)
        {
            return isOpened ? door.frontCloseCinematicController : door.frontOpenCinematicController;
        } else
        {
            return isOpened ? door.backCloseCinematicController : door.backOpenCinematicController;
        }
    }
}
