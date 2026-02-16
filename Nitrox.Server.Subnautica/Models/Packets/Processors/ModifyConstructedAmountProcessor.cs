using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ModifyConstructedAmountProcessor(BuildingManager buildingManager) : BuildingProcessor<ModifyConstructedAmount>(buildingManager)
{
    public override async Task Process(AuthProcessorContext context, ModifyConstructedAmount packet)
    {
        if (BuildingManager.ModifyConstructedAmount(packet))
        {
            await context.SendToOthersAsync(packet);
        }
    }
}
