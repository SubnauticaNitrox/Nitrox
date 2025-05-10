using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BaseDeconstructedProcessor(BuildingManager buildingManager) : IAuthPacketProcessor<BaseDeconstructed>
{
    private readonly BuildingManager buildingManager = buildingManager;

    public async Task Process(AuthProcessorContext context, BaseDeconstructed packet)
    {
        if (buildingManager.ReplaceBaseByGhost(packet))
        {
            await context.ReplyToOthers(packet);
        }
    }
}
