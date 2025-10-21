using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

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

        if (!worldEntityManager.TryDestroyEntity(packet.Id, out Entity entity))
        {
            return;
        }

        if (entity is VehicleEntity vehicleEntity)
        {
            worldEntityManager.MovePlayerChildrenToRoot(vehicleEntity);
        }

        NitroxVector3? lastKnownPosition = null;
        if (entity is WorldEntity worldEntity)
        {
            lastKnownPosition = worldEntity.Transform.Position;
        }

        EntityDestroyed broadcastPacket = new(packet.Id, entity.TechType, lastKnownPosition);

        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            if (player != destroyingPlayer)
            {
                player.SendPacket(broadcastPacket);
            }
        }
    }
}
