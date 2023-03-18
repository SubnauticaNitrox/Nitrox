using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class EntityReparentedProcessor : AuthenticatedPacketProcessor<EntityReparented>
{
    private readonly EntityRegistry entityRegistry;
    private readonly PlayerManager playerManager;

    public EntityReparentedProcessor(EntityRegistry entityRegistry, PlayerManager playerManager)
    {
        this.entityRegistry = entityRegistry;
        this.playerManager = playerManager;
    }

    public override void Process(EntityReparented packet, Player player)
    {
        entityRegistry.ReparentEntity(packet.Id, packet.NewParentId);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
