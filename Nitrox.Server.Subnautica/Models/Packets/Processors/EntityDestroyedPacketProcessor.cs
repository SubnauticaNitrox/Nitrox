using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntityDestroyedPacketProcessor(PlayerService playerService, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<EntityDestroyed>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, EntityDestroyed packet)
    {
        entitySimulation.EntityDestroyed(packet.Id);

        if (worldEntityManager.TryDestroyEntity(packet.Id, out Entity? entity))
        {
            if (entity is VehicleEntity vehicleEntity)
            {
                worldEntityManager.MovePlayerChildrenToRoot(vehicleEntity);
            }

            foreach ((SessionId SessionId, VisibleCellsProperty Property) visibleCells in playerService.GetAll<VisibleCellsProperty>())
            {
                bool isOtherPlayer = visibleCells.SessionId != context.Sender;
                if (isOtherPlayer && visibleCells.Property.CanSee(entity))
                {
                    await context.SendAsync(packet, visibleCells.SessionId);
                }
            }
        }
    }
}
