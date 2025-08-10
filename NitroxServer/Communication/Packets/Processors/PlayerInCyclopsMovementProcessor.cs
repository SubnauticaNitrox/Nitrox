using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class PlayerInCyclopsMovementProcessor : AuthenticatedPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public PlayerInCyclopsMovementProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
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
            Log.ErrorOnce($"{nameof(PlayerEntity)} couldn't be found for player {player.Name}. It is adviced the player reconnects before losing too much progression.");
        }
    }
}
