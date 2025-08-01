using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class EntityDestroyedPacketProcessor : AuthenticatedPacketProcessor<EntityDestroyed>
{
    private readonly PlayerManager playerManager;
    private readonly EntitySimulation entitySimulation;
    private readonly WorldEntityManager worldEntityManager;

    public EntityDestroyedPacketProcessor(PlayerManager playerManager, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager)
    {
        this.playerManager = playerManager;
        this.worldEntityManager = worldEntityManager;
        this.entitySimulation = entitySimulation;
    }

    public override void Process(EntityDestroyed packet, Player destroyingPlayer)
    {
        entitySimulation.EntityDestroyed(packet.Id);

        if (worldEntityManager.TryDestroyEntity(packet.Id, out Entity entity))
        {
            if (entity is VehicleWorldEntity vehicleWorldEntity)
            {
                worldEntityManager.MovePlayerChildrenToRoot(vehicleWorldEntity);
            }

            playerManager.SendPacketToOtherPlayers(packet, destroyingPlayer);
        }
    }
}
