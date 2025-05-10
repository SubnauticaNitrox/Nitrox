using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class UpdateBaseProcessor(GameLogic.Bases.BuildingManager buildingManager, GameLogic.EntitySimulation entitySimulation) : IAuthPacketProcessor<UpdateBase>
{
    public async Task Process(AuthProcessorContext context, UpdateBase packet)
    {
        // TODO: USE DATABASE
        // if (buildingManager.UpdateBase(player, packet, out int operationId))
        // {
        //     if (packet.BuiltPieceEntity is GlobalRootEntity entity)
        //     {
        //         entitySimulation.ClaimBuildPiece(entity, player);
        //     }
        //     // End-players can process elementary operations without this data (packet would be heavier for no reason)
        //     packet.Deflate();
        //     packet.OperationId = operationId;
        //     playerService.SendPacketToOtherPlayers(packet, player);
        // }
    }
}
