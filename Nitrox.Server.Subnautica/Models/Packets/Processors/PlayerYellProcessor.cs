using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerYellProcessor(
    EntityRegistry entityRegistry,
    PlayerManager playerManager,
    ILogger<PlayerYellProcessor> logger) : IAuthPacketProcessor<PlayerYell>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<PlayerYellProcessor> logger = logger;
    private readonly PlayerManager playerManager = playerManager;

    public async Task Process(AuthProcessorContext context, PlayerYell packet)
    {
        if (packet.SoundIndex >= PlayerYell.SOUND_COUNT)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} tried to use invalid yell sound index {packet.SoundIndex}");
            return;
        }

        if (context.Sender.PlayerContext is not { } playerContext ||
            playerContext.IsMuted ||
            playerContext.DrivingVehicle != null)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} tried to yell while muted or driving a vehicle");
            return;
        }

        if (packet.SessionId != context.Sender.SessionId)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} sent a yell with mismatched session id {packet.SessionId}");
        }

        bool isInsideVehicle = playerContext.PassengerSeamoth != null || IsInsideCyclops(context.Sender);
        if (packet.IsInsideVehicle != isInsideVehicle)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} sent a yell with mismatched vehicle state");
        }

        PlayerYell canonicalPacket = new(context.Sender.SessionId, packet.SoundIndex, isInsideVehicle);
        foreach (Player player in playerManager.GetConnectedPlayersExcept(context.Sender))
        {
            if (NitroxVector3.Distance(player.Position, context.Sender.Position) <= PlayerYell.MAX_AUDIBLE_DISTANCE)
            {
                await context.SendAsync(canonicalPacket, player.SessionId);
            }
        }
    }

    private bool IsInsideCyclops(Player player)
    {
        return player.SubRootId.HasValue &&
               entityRegistry.TryGetEntityById(player.SubRootId.Value, out VehicleEntity vehicle) &&
               vehicle.TechType.Name == "Cyclops";
    }
}
