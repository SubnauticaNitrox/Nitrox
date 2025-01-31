using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class UpdateBaseProcessor : BuildingProcessor<UpdateBase>
{
    public UpdateBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation) : base(buildingManager, playerManager, entitySimulation) { }

    public override void Process(UpdateBase packet, Player player)
    {
        if (buildingManager.UpdateBase(player, packet, out int operationId))
        {
            if (packet.BuiltPieceEntity is GlobalRootEntity entity)
            {
                ClaimBuildPiece(entity, player);
            }
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
