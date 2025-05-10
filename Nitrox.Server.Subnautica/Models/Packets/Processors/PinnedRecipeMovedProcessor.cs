using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PinnedRecipeMovedProcessor : IAuthPacketProcessor<PinnedRecipeMoved>
{
    public async Task Process(AuthProcessorContext context, PinnedRecipeMoved packet)
    {
        // TODO: USE DATABASE
        // player.PinnedRecipePreferences.Clear();
        // player.PinnedRecipePreferences.AddRange(packet.RecipePins);
    }
}
