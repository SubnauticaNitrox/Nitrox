using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerInCyclopsMovementProcessor(EntityRegistry entityRegistry, PlayerService playerService, ILogger<PlayerInCyclopsMovementProcessor> logger) : IAuthPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<PlayerInCyclopsMovementProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PlayerInCyclopsMovement packet)
    {
        if (!entityRegistry.TryGetEntityById(playerService.GetProperty<GameObjectIdProperty>(context.Sender).Value, out PlayerEntity playerEntity))
        {
            logger.ZLogErrorOnce($"{nameof(PlayerEntity)} couldn't be found for player {playerService.GetProperty<NameProperty>(context.Sender).Value}. It is advised the player reconnects before losing too much progression.");
            return;
        }

        playerEntity.Transform.LocalPosition = packet.LocalPosition;
        playerEntity.Transform.LocalRotation = packet.LocalRotation;
        playerService.GetProperty<PositionProperty>(context.Sender).Value = playerEntity.Transform.Position;
        playerService.GetProperty<RotationProperty>(context.Sender).Value = playerEntity.Transform.Rotation;
        await context.SendToOthersAsync(packet);
    }
}
