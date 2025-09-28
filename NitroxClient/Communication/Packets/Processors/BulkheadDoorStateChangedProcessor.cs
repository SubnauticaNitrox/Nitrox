using System.Data.Common;
using System.Diagnostics.SymbolStore;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.CinematicController;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

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

        otherPlayer.AnimationController["player_in_front"] = packet.IsFacingDoor;


        // Get the BulkheadDoor component
        if (bulkheadDoor.TryGetComponentInChildren(out BulkheadDoor door))
        {
            Log.Info("Found BulkheadDoor");

            bool prevState = !packet.IsOpen;

            PlayerCinematicController cinematicController = GetPlayerCinematicController(door, prevState, packet.IsFacingDoor);
            MultiplayerCinematicController multiPlayerCinematicController = MultiplayerCinematicController.Initialize(cinematicController);

            multiPlayerCinematicController.CallStartCinematicMode(otherPlayer);

            door.SetState(packet.IsOpen);


            Log.Info($"[BulkheadDoorStateChangedProcessor] Initialized and started/ended cinematic for remote player {otherPlayer.PlayerName}");
        }
        else
        {
            Log.Info("Did NOT find BulkheadDoor");
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
