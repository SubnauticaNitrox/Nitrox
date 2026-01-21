using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerMovementProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<PlayerMovement>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, PlayerMovement packet)
    {
        Optional<PlayerEntity> playerEntity = entityRegistry.GetEntityById<PlayerEntity>(context.Sender.PlayerContext.PlayerNitroxId);

        if (playerEntity.HasValue)
        {
            playerEntity.Value.Transform.Position = packet.Position;
            playerEntity.Value.Transform.Rotation = packet.BodyRotation;
        }

        context.Sender.Position = packet.Position;
        context.Sender.Rotation = packet.BodyRotation;
        await context.SendToOthersAsync(packet);
    }
}
