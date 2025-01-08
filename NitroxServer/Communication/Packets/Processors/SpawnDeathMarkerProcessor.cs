using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;
public class SpawnDeathMarkerProcessor : AuthenticatedPacketProcessor<SpawnDeathMarker>
{
    private readonly PlayerManager playerManager;

    public SpawnDeathMarkerProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(SpawnDeathMarker packet, Player player)
    {
        playerManager.SendPacketToAllPlayers(packet);
    }
}
