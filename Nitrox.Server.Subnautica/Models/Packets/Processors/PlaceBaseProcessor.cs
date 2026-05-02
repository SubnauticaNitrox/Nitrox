using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceBaseProcessor(BuildingManager buildingManager, EntitySimulation entitySimulation) : BuildingProcessor<PlaceBase>(buildingManager, entitySimulation)
{
    public override async Task Process(AuthProcessorContext context, PlaceBase packet)
    {
        if (BuildingManager.CreateBase(packet))
        {
            await TryClaimBuildPieceAsync(context, packet.BuildEntity);
            
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            await context.SendToOthersAsync(packet);
        }
    }
}
