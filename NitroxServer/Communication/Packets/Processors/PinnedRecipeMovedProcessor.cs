using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class PinnedRecipeMovedProcessor : AuthenticatedPacketProcessor<PinnedRecipeMoved>
{
    public override void Process(PinnedRecipeMoved packet, Player player)
    {
        player.PinnedRecipePreferences.Clear();
        player.PinnedRecipePreferences.AddRange(packet.RecipePins);
    }
}
