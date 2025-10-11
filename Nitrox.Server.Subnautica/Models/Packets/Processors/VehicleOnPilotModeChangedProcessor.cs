using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class VehicleOnPilotModeChangedProcessor : AuthenticatedPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly PlayerManager playerManager;

    public VehicleOnPilotModeChangedProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(VehicleOnPilotModeChanged packet, Player player)
    {
        player.PlayerContext.DrivingVehicle = packet.IsPiloting ? packet.VehicleId : null;

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
