using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PinnedRecipeProcessor : IAuthPacketProcessor<RecipePinned>
{
    public async Task Process(AuthProcessorContext context, RecipePinned packet)
    {
        // TODO: USE DATABASE
        // if (packet.Pinned)
        // {
        //     player.PinnedRecipePreferences.Add(packet.TechType);
        // }
        // else
        // {
        //     player.PinnedRecipePreferences.Remove(packet.TechType);
        // }
    }
}
