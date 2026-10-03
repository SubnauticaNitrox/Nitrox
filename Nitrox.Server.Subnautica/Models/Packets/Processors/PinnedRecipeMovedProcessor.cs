using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PinnedRecipeMovedProcessor(PlayerService playerService) : IAuthPacketProcessor<PinnedRecipeMoved>
{
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, PinnedRecipeMoved packet)
    {
        playerService.GetProperty<PinnedRecipesProperty>(context.Sender).Value.ClearAndSet(packet.RecipePins);
        return Task.CompletedTask;
    }
}
