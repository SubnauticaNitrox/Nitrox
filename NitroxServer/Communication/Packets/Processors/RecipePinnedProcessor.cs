using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class RecipePinnedProcessor : AuthenticatedPacketProcessor<RecipePinned>
{
    public override void Process(RecipePinned packet, Player player)
    {
        if (packet.Pinned)
        {
            player.PinPreferences.Add(packet.TechType);
        }
        else
        {
            player.PinPreferences.Remove(packet.TechType);
        }        
    }
}
