using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PinnedRecipeMovedProcessor : IAuthPacketProcessor<PinnedRecipeMoved>
{
    public Task Process(AuthProcessorContext context, PinnedRecipeMoved packet)
    {
        context.Sender.PinnedRecipePreferences.Clear();
        context.Sender.PinnedRecipePreferences.AddRange(packet.RecipePins);
        return Task.CompletedTask;
    }
}
