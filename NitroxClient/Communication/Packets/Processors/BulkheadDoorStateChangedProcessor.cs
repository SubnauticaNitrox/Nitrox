using System.Data.Common;
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

        //otherPlayer.AnimationController["cinematics_enabled"] = true;
        //otherPlayer.AnimationController["door_use"] = true;
        //otherPlayer.AnimationController["door_opening"] = packet.IsOpen;
        //otherPlayer.AnimationController["door_closing"] = !packet.IsOpen;

        otherPlayer.AnimationController["opened"] = packet.IsOpen;
        otherPlayer.AnimationController["player_in_front"] = packet.IsOpen;


        // Get the BulkheadDoor component
        if (bulkheadDoor.TryGetComponentInChildren(out BulkheadDoor door))
        {
            Log.Info("Found BulkheadDoor");

            MultiplayerCinematicController newController = MultiplayerCinematicController.Initialize(packet.IsOpen ? door.frontOpenCinematicController : door.frontCloseCinematicController);

            if (packet.IsOpen)
            {
                newController.CallStartCinematicMode(otherPlayer);
            }
            else
            {
                newController.CallCinematicModeEnd(otherPlayer);
            }
            Log.Info($"[BulkheadDoorStateChangedProcessor] Initialized and started/ended cinematic for remote player {otherPlayer.PlayerName}");
        }
        else
        {
            Log.Info("Did NOT find BulkheadDoor");
        }
    }
}
