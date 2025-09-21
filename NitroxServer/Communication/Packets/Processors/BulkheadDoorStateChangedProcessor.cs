using NitroxServer;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

public class BulkheadDoorStateChangedProcessor : AuthenticatedPacketProcessor<BulkheadDoorStateChanged>
{

    private readonly PlayerManager playerManager;

    public BulkheadDoorStateChangedProcessor(PlayerManager playerManager) {
        this.playerManager = playerManager;
    }


    public override void Process(BulkheadDoorStateChanged packet, Player player)
    {
        // Broadcast the door state change to all other players
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
