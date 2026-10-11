using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerMovementProcessor(EntityRegistry entityRegistry, PlayerService playerService) : IAuthPacketProcessor<PlayerMovement>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, PlayerMovement packet)
    {
        Optional<PlayerEntity> playerEntity = entityRegistry.GetEntityById<PlayerEntity>(playerService.Get<GameObjectIdProperty>(context.Sender).Value);

        if (playerEntity.HasValue)
        {
            playerEntity.Value.Transform.Position = packet.Position;
            playerEntity.Value.Transform.Rotation = packet.BodyRotation;
        }

        playerService.Get<PositionProperty>(context.Sender).Value = packet.Position;
        playerService.Get<RotationProperty>(context.Sender).Value = packet.BodyRotation;
        await context.SendToOthersAsync(packet);
    }
}
