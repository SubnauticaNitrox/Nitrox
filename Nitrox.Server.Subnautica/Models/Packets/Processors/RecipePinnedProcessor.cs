using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PinnedRecipeProcessor : IAuthPacketProcessor<RecipePinned>
{
    public Task Process(AuthProcessorContext context, RecipePinned packet)
    {
        if (packet.Pinned)
        {
            context.Sender.PinnedRecipePreferences.Add(packet.TechType);
        }
        else
        {
            context.Sender.PinnedRecipePreferences.Remove(packet.TechType);
        }
        return Task.CompletedTask;
    }
}
