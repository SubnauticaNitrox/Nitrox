using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerInCyclopsMovementProcessor : AuthenticatedPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;
    private readonly ILogger<PlayerInCyclopsMovementProcessor> logger;

    public PlayerInCyclopsMovementProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, ILogger<PlayerInCyclopsMovementProcessor> logger)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
        this.logger = logger;
    }

    public override void Process(PlayerInCyclopsMovement packet, Player player)
    {
        if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerEntity playerEntity))
        {
            playerEntity.Transform.LocalPosition = packet.LocalPosition;
            playerEntity.Transform.LocalRotation = packet.LocalRotation;

            player.Position = playerEntity.Transform.Position;
            player.Rotation = playerEntity.Transform.Rotation;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
        else
        {
            logger.ZLogErrorOnce($"{nameof(PlayerEntity)} couldn't be found for player {player.Name}. It is advised the player reconnects before losing too much progression.");
        }
    }
}
