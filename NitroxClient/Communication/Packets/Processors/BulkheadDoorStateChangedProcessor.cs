using System.Collections;
using System.Data.Common;
using System.Diagnostics.SymbolStore;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
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
        Log.Info($"[BulkheadDoorStateChangedProcessor] Received bulkhead door state change: {packet.Id} -> {(packet.IsOpen ? "OPEN" : "CLOSED")}");

        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject bulkheadDoor))
        {
            Log.Error($"[BulkheadDoorStateChangedProcessor] Couldn't find GameObject for {packet.Id}");
            return;
        }

        Log.Info($"[BulkheadDoorStateChangedProcessor] GameObject id: {bulkheadDoor.GetId().Value} Name: {bulkheadDoor.name}");

        if (!remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer otherPlayer))
        {
            Log.Error($"Couldn't find {nameof(RemotePlayer)} for {packet.PlayerId}");
            return;
        }

        Log.Info($"[BulkheadDoorStateChangedProcessor] RemotePlayer PlayerName: {otherPlayer.PlayerName} PlayerId: {otherPlayer.PlayerId}");

        // Get the BulkheadDoor component
        if (bulkheadDoor.TryGetComponentInChildren(out BulkheadDoor door))
        {
            Log.Info("Found BulkheadDoor");

            bool prevState = !packet.IsOpen;

            door.animator.SetBool("player_in_front", packet.IsFacingDoor);

            PlayerCinematicController cinematicController = GetPlayerCinematicController(door, prevState, packet.IsFacingDoor);
            MultiplayerCinematicController multiPlayerCinematicController = MultiplayerCinematicController.Initialize(cinematicController);

            multiPlayerCinematicController.CallStartCinematicMode(otherPlayer);

            CoroutineHost.StartCoroutine(SetDoorStateAfterDelay(door, packet.IsOpen, 3.0f));

            Log.Info($"[BulkheadDoorStateChangedProcessor] Initialized and started/ended cinematic for remote player {otherPlayer.PlayerName}");
        }
        else
        {
            Log.Info("Did NOT find BulkheadDoor");
        }
    }

    private IEnumerator SetDoorStateAfterDelay(BulkheadDoor door, bool isOpen, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (door != null)
        {
            door.SetState(isOpen);
            Log.Info($"[BulkheadDoorStateChangedProcessor] Set door state to {(isOpen ? "OPEN" : "CLOSED")} after cinematic delay");
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
