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

        CoroutineHost.StartCoroutine(SetDoorStateAfterDelay(door, packet.IsOpen, 3.0f));
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
