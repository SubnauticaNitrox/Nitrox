using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WaterParkDeconstructedProcessor(BuildingManager buildingManager) : BuildingProcessor<WaterParkDeconstructed>(buildingManager)
{
    public override async Task Process(AuthProcessorContext context, WaterParkDeconstructed packet)
    {
        if (BuildingManager.ReplacePieceByGhost(context.Sender, packet, out Entity removedEntity, out int operationId) &&
            BuildingManager.CreateWaterParkPiece(packet, removedEntity))
        {
            packet.BaseData = null;
            await SendToOtherPlayersWithOperationIdAsync(context, packet, operationId);
        }
    }
}
