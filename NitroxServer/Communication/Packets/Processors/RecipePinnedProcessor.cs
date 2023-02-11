using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class PinnedRecipeProcessor : AuthenticatedPacketProcessor<RecipePinned>
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
