using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class SignalPingPreferenceChangedProcessor : AuthenticatedPacketProcessor<SignalPingPreferenceChanged>
{
    public override void Process(SignalPingPreferenceChanged packet, Player player)
    {
        if (packet.Visible.HasValue)
        {
            player.SetPingVisible(packet.PingKey, packet.Visible.Value);
        }
        if (packet.Color.HasValue)
        {
            player.SetPingColor(packet.PingKey, packet.Color.Value);
        }
    }
}
