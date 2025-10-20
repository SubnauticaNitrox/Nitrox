using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlaceModuleProcessor : BuildingProcessor<PlaceModule>
{
    public PlaceModuleProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, playerManager, entitySimulation) { }

    public override void Process(PlaceModule packet, Player player)
    {
        if (buildingManager.AddModule(packet))
        {
            if (packet.ModuleEntity.ParentId == null || !packet.ModuleEntity.IsInside)
            {
                TryClaimBuildPiece(packet.ModuleEntity, player);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
