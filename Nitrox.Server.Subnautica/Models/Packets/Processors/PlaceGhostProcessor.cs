using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

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
