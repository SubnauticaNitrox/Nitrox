using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor : IAuthPacketProcessor<VehicleOnPilotModeChanged>
{
    public async Task Process(AuthProcessorContext context, VehicleOnPilotModeChanged packet)
    {
        // TODO: USE DATABASE
        // player.PlayerContext.DrivingVehicle = packet.IsPiloting ? packet.VehicleId : null;

        await context.ReplyToOthers(packet);
    }
}
