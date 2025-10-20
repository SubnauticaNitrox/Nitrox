using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlaceBaseProcessor : BuildingProcessor<PlaceBase>
{
    public PlaceBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, playerManager, entitySimulation){ }

    public override void Process(PlaceBase packet, Player player)
    {
        if (buildingManager.CreateBase(packet))
        {
            TryClaimBuildPiece(packet.BuildEntity, player);
            
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
