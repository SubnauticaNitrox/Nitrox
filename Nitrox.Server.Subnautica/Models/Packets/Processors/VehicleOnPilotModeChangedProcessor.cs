using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor(IPacketSender packetSender) : AuthenticatedPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(VehicleOnPilotModeChanged packet, Player player)
    {
        player.PlayerContext.DrivingVehicle = packet.IsPiloting ? packet.VehicleId : null;

        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
