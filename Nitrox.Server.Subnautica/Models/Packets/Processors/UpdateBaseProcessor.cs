using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class UpdateBaseProcessor : BuildingProcessor<UpdateBase>
{
    public UpdateBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, playerManager, entitySimulation) { }

    public override void Process(UpdateBase packet, Player player)
    {
        if (buildingManager.UpdateBase(player, packet, out int operationId))
        {
            if (packet.BuiltPieceEntity is GlobalRootEntity entity)
            {
                TryClaimBuildPiece(entity, player);
            }
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
