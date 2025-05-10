using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerInCyclopsMovementProcessor(EntityRegistry entityRegistry, ILogger<PlayerInCyclopsMovementProcessor> logger) : IAuthPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<PlayerInCyclopsMovementProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PlayerInCyclopsMovement packet)
    {
        // TODO: USE DATABASE (storing nitrox id of player in session record)
        // if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerWorldEntity playerWorldEntity))
        // {
        //     playerWorldEntity.Transform.LocalPosition = packet.LocalPosition;
        //     playerWorldEntity.Transform.LocalRotation = packet.LocalRotation;
        //
        //     player.Position = playerWorldEntity.Transform.Position;
        //     player.Rotation = playerWorldEntity.Transform.Rotation;
        //     playerService.SendPacketToOtherPlayers(packet, player);
        // }
        // else
        // {
        //     logger.LogErrorOnce("{TypeName} couldn't be found for player {PlayerName}. It is advised the player reconnects before losing too much progression.", nameof(PlayerWorldEntity), player.Name);
        // }
    }
}
