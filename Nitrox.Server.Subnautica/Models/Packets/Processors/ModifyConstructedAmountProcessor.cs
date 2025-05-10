using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ModifyConstructedAmountProcessor(GameLogic.Bases.BuildingManager buildingManager) : IAuthPacketProcessor<ModifyConstructedAmount>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;

    public async Task Process(AuthProcessorContext context, ModifyConstructedAmount packet)
    {
        if (buildingManager.ModifyConstructedAmount(packet))
        {
            await context.ReplyToOthers(packet);
        }
    }
}
