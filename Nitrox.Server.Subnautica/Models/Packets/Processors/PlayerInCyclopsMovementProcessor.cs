using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerInCyclopsMovementProcessor(IPacketSender packetSender, EntityRegistry entityRegistry, ILogger<PlayerInCyclopsMovementProcessor> logger) : IAuthPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<PlayerInCyclopsMovementProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PlayerInCyclopsMovement packet)
    {
        if (!entityRegistry.TryGetEntityById(context.Sender.PlayerContext.PlayerNitroxId, out PlayerEntity playerEntity))
        {
            logger.ZLogErrorOnce($"{nameof(PlayerEntity)} couldn't be found for player {context.Sender.Name}. It is advised the player reconnects before losing too much progression.");
            return;
        }

        playerEntity.Transform.LocalPosition = packet.LocalPosition;
        playerEntity.Transform.LocalRotation = packet.LocalRotation;
        context.Sender.Position = playerEntity.Transform.Position;
        context.Sender.Rotation = playerEntity.Transform.Rotation;
        await context.SendToOthersAsync(packet);
    }
}
