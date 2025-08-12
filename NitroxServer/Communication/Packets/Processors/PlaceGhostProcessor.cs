using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors;

public class PlaceGhostProcessor : BuildingProcessor<PlaceGhost>
{
    public PlaceGhostProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(PlaceGhost packet, Player player)
    {
        if (buildingManager.AddGhost(packet))
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
