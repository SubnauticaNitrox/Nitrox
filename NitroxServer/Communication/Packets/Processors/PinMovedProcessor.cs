using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class PinMovedProcessor : AuthenticatedPacketProcessor<PinMoved>
{
    public override void Process(PinMoved packet, Player player)
    {
        player.PinPreferences.Clear();
        player.PinPreferences.AddRange(packet.Pins);
    }
}
