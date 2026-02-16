using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceGhostProcessor(BuildingManager buildingManager) : BuildingProcessor<PlaceGhost>(buildingManager)
{
    public override async Task Process(AuthProcessorContext context, PlaceGhost packet)
    {
        if (BuildingManager.AddGhost(packet))
        {
            await context.SendToOthersAsync(packet);
        }
    }
}
