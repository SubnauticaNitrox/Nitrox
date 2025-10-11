using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PinnedRecipeMovedProcessor : AuthenticatedPacketProcessor<PinnedRecipeMoved>
{
    public override void Process(PinnedRecipeMoved packet, Player player)
    {
        player.PinnedRecipePreferences.Clear();
        player.PinnedRecipePreferences.AddRange(packet.RecipePins);
    }
}
