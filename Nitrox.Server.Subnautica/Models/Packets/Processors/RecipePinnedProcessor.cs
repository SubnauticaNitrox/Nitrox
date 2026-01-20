using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PinnedRecipeProcessor : AuthenticatedPacketProcessor<RecipePinned>
{
    public override void Process(RecipePinned packet, Player player)
    {
        if (packet.Pinned)
        {
            player.PinnedRecipePreferences.Add(packet.TechType);
        }
        else
        {
            player.PinnedRecipePreferences.Remove(packet.TechType);
        }        
    }
}
