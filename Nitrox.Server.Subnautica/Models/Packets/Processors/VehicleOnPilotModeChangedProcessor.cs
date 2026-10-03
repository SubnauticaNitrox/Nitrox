using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor(PlayerService playerService) : IAuthPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, VehicleOnPilotModeChanged packet)
    {
        playerService.GetProperty<DrivingVehicleProperty>(context.Sender).Value = packet.IsPiloting ? packet.VehicleId : null;
        await context.SendToOthersAsync(packet);
    }
}
