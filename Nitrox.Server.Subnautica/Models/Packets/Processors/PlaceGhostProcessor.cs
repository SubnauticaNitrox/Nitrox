using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceGhostProcessor(GameLogic.Bases.BuildingManager buildingManager) : IAuthPacketProcessor<PlaceGhost>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;

    public async Task Process(AuthProcessorContext context, PlaceGhost packet)
    {
        if (buildingManager.AddGhost(packet))
        {
            await context.ReplyToOthers(packet);
        }
    }
}
