using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceModuleProcessor(BuildingManager buildingManager, EntitySimulation entitySimulation) : BuildingProcessor<PlaceModule>(buildingManager, entitySimulation)
{
    public override async Task Process(AuthProcessorContext context, PlaceModule packet)
    {
        if (BuildingManager.AddModule(packet))
        {
            if (packet.ModuleEntity.ParentId == null || !packet.ModuleEntity.IsInside)
            {
                await TryClaimBuildPieceAsync(context, packet.ModuleEntity);
            }
            await context.SendToOthersAsync(packet);
        }
    }
}
