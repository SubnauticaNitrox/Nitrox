using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SignalPingPreferenceChangedProcessor : IAuthPacketProcessor<SignalPingPreferenceChanged>
{
    public Task Process(AuthProcessorContext context, SignalPingPreferenceChanged packet)
    {
        context.Sender.PingInstancePreferences[packet.PingKey] = new(packet.Color, packet.Visible);
        return Task.CompletedTask;
    }
}
