using System.Collections.Concurrent;
using System.Diagnostics;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleHornProcessor(EntityRegistry entityRegistry, PlayerManager playerManager, ILogger<VehicleHornProcessor> logger) : IAuthPacketProcessor<VehicleHorn>
{
    private static readonly long CooldownTicks = (long)(VehicleHorn.COOLDOWN_SECONDS * Stopwatch.Frequency);

    private readonly ConcurrentDictionary<NitroxId, long> lastHornTimestamps = new();
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

        if (!TryBeginCooldown(packet.VehicleId, Stopwatch.GetTimestamp()))
        {
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

    internal bool TryBeginCooldown(NitroxId vehicleId, long now)
    {
        while (true)
        {
            if (!lastHornTimestamps.TryGetValue(vehicleId, out long previousTimestamp))
            {
                if (lastHornTimestamps.TryAdd(vehicleId, now))
                {
                    return true;
                }
                continue;
            }

            if (now - previousTimestamp < CooldownTicks)
            {
                return false;
            }

            if (lastHornTimestamps.TryUpdate(vehicleId, now, previousTimestamp))
            {
                return true;
            }
        }
    }

    private static bool IsSupportedVehicle(VehicleEntity vehicle)
    {
        return vehicle.TechType.Name is "Seamoth" or "Cyclops";
    }
}
