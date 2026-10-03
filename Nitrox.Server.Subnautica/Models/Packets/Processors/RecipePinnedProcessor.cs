using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PinnedRecipeProcessor(PlayerService playerService) : IAuthPacketProcessor<RecipePinned>
{
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, RecipePinned packet)
    {
        PinnedRecipesProperty senderPinnedRecipes = playerService.GetProperty<PinnedRecipesProperty>(context.Sender);
        if (packet.Pinned)
        {
            senderPinnedRecipes.Value.Add(packet.TechType);
        }
        else
        {
            senderPinnedRecipes.Value.Remove(packet.TechType);
        }
        return Task.CompletedTask;
    }
}
