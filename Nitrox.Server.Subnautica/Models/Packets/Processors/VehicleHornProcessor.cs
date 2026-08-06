using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleHornProcessor(EntityRegistry entityRegistry, PlayerManager playerManager, ILogger<VehicleHornProcessor> logger) : IAuthPacketProcessor<VehicleHorn>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<VehicleHornProcessor> logger = logger;
    private readonly PlayerManager playerManager = playerManager;

    public async Task Process(AuthProcessorContext context, VehicleHorn packet)
    {
        if (context.Sender.PlayerContext?.DrivingVehicle != packet.VehicleId)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} tried to use the horn on {packet.VehicleId} without piloting it");
            return;
        }

        if (!entityRegistry.TryGetEntityById(packet.VehicleId, out VehicleEntity vehicle) || !IsSupportedVehicle(vehicle))
        {
            logger.ZLogWarning($"Player {context.Sender.Name} tried to use a horn on unsupported or unknown vehicle {packet.VehicleId}");
            return;
        }

        NitroxVector3 hornPosition = vehicle.Transform.Position;
        foreach (Player player in playerManager.GetConnectedPlayersExcept(context.Sender))
        {
            if (NitroxVector3.Distance(player.Position, hornPosition) <= VehicleHorn.MAX_AUDIBLE_DISTANCE)
            {
                await context.SendAsync(packet, player.SessionId);
            }
        }
    }

    private static bool IsSupportedVehicle(VehicleEntity vehicle)
    {
        return vehicle.TechType.Name is "Seamoth" or "Cyclops";
    }
}
