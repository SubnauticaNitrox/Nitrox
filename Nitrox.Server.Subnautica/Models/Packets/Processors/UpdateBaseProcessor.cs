using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class UpdateBaseProcessor(BuildingManager buildingManager, EntitySimulation entitySimulation) : BuildingProcessor<UpdateBase>(buildingManager, entitySimulation)
{
    public override async Task Process(AuthProcessorContext context, UpdateBase packet)
    {
        if (BuildingManager.UpdateBase(context.Sender, packet, out int operationId))
        {
            if (packet.BuiltPieceEntity is GlobalRootEntity entity)
            {
                await TryClaimBuildPieceAsync(context, entity);
            }
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            await SendToOtherPlayersWithOperationIdAsync(context, packet, operationId);
        }
    }
}
