using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class BasicMovementPacketProcessor : AuthenticatedPacketProcessor<BasicMovement>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public BasicMovementPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(BasicMovement packet, Player player)
    {
        if (entityRegistry.TryGetEntityById(packet.Id, out WorldEntity entity))
        {
            entity.Transform.Position = packet.Position;
            entity.Transform.Rotation = packet.Rotation;
        }

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
