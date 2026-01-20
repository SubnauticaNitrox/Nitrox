using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerInCyclopsMovementProcessor(IPacketSender packetSender, EntityRegistry entityRegistry, ILogger<PlayerInCyclopsMovementProcessor> logger) : AuthenticatedPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<PlayerInCyclopsMovementProcessor> logger = logger;

    public override void Process(PlayerInCyclopsMovement packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerEntity playerEntity))
        {
            logger.ZLogErrorOnce($"{nameof(PlayerEntity)} couldn't be found for player {player.Name}. It is advised the player reconnects before losing too much progression.");
            return;
        }

        playerEntity.Transform.LocalPosition = packet.LocalPosition;
        playerEntity.Transform.LocalRotation = packet.LocalRotation;
        player.Position = playerEntity.Transform.Position;
        player.Rotation = playerEntity.Transform.Rotation;
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
