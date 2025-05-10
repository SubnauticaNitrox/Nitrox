using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class SignalPingPreferenceChangedProcessor : IAuthPacketProcessor<SignalPingPreferenceChanged>
{
    public async Task Process(AuthProcessorContext context, SignalPingPreferenceChanged packet)
    {
        // TODO: USE DATABASE
        // player.PingInstancePreferences[packet.PingKey] = new(packet.Color, packet.Visible);
    }
}
