using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class LargeWaterParkDeconstructedProcessor(BuildingManager buildingManager) : BuildingProcessor<LargeWaterParkDeconstructed>(buildingManager)
{
    public override async Task Process(AuthProcessorContext context, LargeWaterParkDeconstructed packet)
    {
        // SeparateChildrenToWaterParks must happen before ReplacePieceByGhost
        // so the water park's children can be moved before it being removed
        if (BuildingManager.SeparateChildrenToWaterParks(packet) &&
            BuildingManager.ReplacePieceByGhost(context.Sender, packet, out _, out int operationId))
        {
            packet.BaseData = null;
            await SendToOtherPlayersWithOperationIdAsync(context, packet, operationId);
        }
    }
}
