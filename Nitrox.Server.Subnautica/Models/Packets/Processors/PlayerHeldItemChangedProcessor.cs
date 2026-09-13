using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerHeldItemChangedProcessor(PlayerService playerService) : IAuthPacketProcessor<PlayerHeldItemChanged>
{
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, PlayerHeldItemChanged packet)
    {
        if (packet.IsFirstTime != null)
        {
            playerService.GetProperty<UsedItemsProperty>(context.Sender).Value.Add(packet.IsFirstTime);
        }

        await context.SendToOthersAsync(packet);
    }
}
