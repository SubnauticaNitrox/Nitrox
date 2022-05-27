using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors;

public class SignalPingPreferenceChangedProcessor : AuthenticatedPacketProcessor<SignalPingPreferenceChanged>
{
    public override void Process(SignalPingPreferenceChanged packet, Player player)
    {
        player.PingInstancePreferences[packet.PingKey] = new(packet.Color, packet.Visible);
    }
}
