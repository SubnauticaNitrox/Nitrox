using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class SignalPingPreferenceChangedProcessor : AuthenticatedPacketProcessor<SignalPingPreferenceChanged>
{
    public override void Process(SignalPingPreferenceChanged packet, Player player)
    {
        player.PingInstancePreferences[packet.PingKey] = new(packet.Color, packet.Visible);
    }
}
