using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor(
    SimulationOwnershipData simulationOwnershipData,
    SeamothPassengerService passengerService) : IAuthPacketProcessor<VehicleOnPilotModeChanged>
{
    public async Task Process(AuthProcessorContext context, VehicleOnPilotModeChanged packet)
    {
        if (packet.IsPiloting)
        {
            if (!simulationOwnershipData.TryGetLock(packet.VehicleId, out SimulationOwnershipData.PlayerLock playerLock) ||
                playerLock.LockType != SimulationLockType.EXCLUSIVE ||
                playerLock.Player != context.Sender)
            {
                return;
            }
        }
        else if (context.Sender.PlayerContext.DrivingVehicle?.Equals(packet.VehicleId) != true)
        {
            return;
        }

        foreach (SeamothPassengerStateChanged state in passengerService.HandlePilotModeChanged(context.Sender, packet.VehicleId, packet.IsPiloting))
        {
            await context.SendToAllAsync(state);
        }

        context.Sender.PlayerContext.DrivingVehicle = packet.IsPiloting ? packet.VehicleId : null;
        VehicleOnPilotModeChanged canonicalPacket = new(packet.VehicleId, context.Sender.SessionId, packet.IsPiloting);
        await context.SendToOthersAsync(canonicalPacket);
    }
}
