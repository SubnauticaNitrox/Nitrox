using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

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
