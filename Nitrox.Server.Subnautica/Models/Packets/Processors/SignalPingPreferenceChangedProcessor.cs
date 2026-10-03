using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SignalPingPreferenceChangedProcessor(PlayerService playerService) : IAuthPacketProcessor<SignalPingPreferenceChanged>
{
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, SignalPingPreferenceChanged packet)
    {
        playerService.GetProperty<PingsProperty>(context.Sender).Value[packet.PingKey] = new(packet.Color, packet.Visible);
        return Task.CompletedTask;
    }
}
