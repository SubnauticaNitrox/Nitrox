using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class SignalPingVisibilityChangedProcessor : AuthenticatedPacketProcessor<SignalPingVisibilityChanged>
{
    public override void Process(SignalPingVisibilityChanged packet, Player player)
    {
        player.SetPingVisible(packet.PingKey, packet.Visible);
    }
}
