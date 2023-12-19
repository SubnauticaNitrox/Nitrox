using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class PlaceModuleProcessor : BuildingProcessor<PlaceModule>
{
    public PlaceModuleProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, playerManager, entitySimulation) { }

    public override void Process(PlaceModule packet, Player player)
    {
        if (buildingManager.AddModule(packet))
        {
            if (packet.ModuleEntity.ParentId == null)
            {
                ClaimBuildPiece(packet.ModuleEntity, player);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
