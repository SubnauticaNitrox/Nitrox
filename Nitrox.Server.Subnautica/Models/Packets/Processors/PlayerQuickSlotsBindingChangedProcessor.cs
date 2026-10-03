using System.Linq;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerQuickSlotsBindingChangedProcessor(PlayerService playerService) : IAuthPacketProcessor<PlayerQuickSlotsBindingChanged>
{
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, PlayerQuickSlotsBindingChanged packet)
    {
        playerService.GetProperty<QuickSlotsProperty>(context.Sender).Value.ClearAndSet(packet.SlotItemIds.Select(id => id.HasValue ? id.Value : null));
        return Task.CompletedTask;
    }
}
