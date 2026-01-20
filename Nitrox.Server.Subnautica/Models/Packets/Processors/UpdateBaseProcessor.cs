using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class UpdateBaseProcessor(BuildingManager buildingManager, IPacketSender packetSender, EntitySimulation entitySimulation) : BuildingProcessor<UpdateBase>(buildingManager, packetSender, entitySimulation)
{
    public override void Process(UpdateBase packet, Player player)
    {
        if (BuildingManager.UpdateBase(player, packet, out int operationId))
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
