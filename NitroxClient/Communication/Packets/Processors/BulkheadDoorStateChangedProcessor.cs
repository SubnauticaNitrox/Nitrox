using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
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
        Log.Info($"[Client] Received bulkhead door state change: {packet.Id} -> {(packet.IsOpen ? "OPEN" : "CLOSED")}");

        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject bulkheadDoor))
        {
            Log.Error($"Couldn't find GameObject for {packet.Id}");
            return;
        }

        if (!remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer remotePlayer))
        {
            Log.Error($"Couldn't find {nameof(RemotePlayer)} for {packet.PlayerId}");
            return;
        }

        // Apply player animations for door interaction (like bench does)
        remotePlayer.AnimationController["cinematics_enabled"] = true;

        if (bulkheadDoor.TryGetComponent<BulkheadDoor>(out BulkheadDoor door))
        {
            Log.Info($"[Client] Player {remotePlayer.PlayerName} is animating bulkhead door {packet.Id}");
        }

    }
}
