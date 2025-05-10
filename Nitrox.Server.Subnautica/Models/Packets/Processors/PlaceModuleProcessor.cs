using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceModuleProcessor(GameLogic.Bases.BuildingManager buildingManager, GameLogic.EntitySimulation entitySimulation) : IAuthPacketProcessor<PlaceModule>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;
    private readonly GameLogic.EntitySimulation entitySimulation = entitySimulation;

    public async Task Process(AuthProcessorContext context, PlaceModule packet)
    {
        if (buildingManager.AddModule(packet))
        {
            if (packet.ModuleEntity.ParentId == null)
            {
                await entitySimulation.ClaimBuildPiece(packet.ModuleEntity, context.Sender.PlayerId);
            }
            await context.ReplyToOthers(packet);
        }
    }
}
