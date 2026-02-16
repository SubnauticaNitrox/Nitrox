using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor : IAuthPacketProcessor<VehicleOnPilotModeChanged>
{
    public async Task Process(AuthProcessorContext context, VehicleOnPilotModeChanged packet)
    {
        context.Sender.PlayerContext.DrivingVehicle = packet.IsPiloting ? packet.VehicleId : null;
        await context.SendToOthersAsync(packet);
    }
}
