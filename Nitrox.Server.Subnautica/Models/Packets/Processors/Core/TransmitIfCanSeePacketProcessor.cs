using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

internal abstract class TransmitIfCanSeePacketProcessor<T>(PlayerService playerService, EntityRegistry entityRegistry) : IAuthPacketProcessor<T>
    where T : Packet
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    /// <summary>
    /// Transmits the provided <paramref name="packet"/> to all other players (excluding <paramref name="context"/> sender)
    /// who can see (<see cref="VisibleCellsProperty.CanSee"/>) entities corresponding to the provided <paramref name="entityIds"/> only if all those entities are registered.
    /// </summary>
    protected async Task TransmitIfCanSeeEntitiesAsync(AuthProcessorContext context, Packet packet, List<NitroxId> entityIds)
    {
        List<Entity> entities = [];
        foreach (NitroxId entityId in entityIds)
        {
            if (entityRegistry.TryGetEntityById(entityId, out Entity entity))
            {
                entities.Add(entity);
            }
            else
            {
                return;
            }
        }

        foreach (SessionId player in playerService.GetSessionsExcept(context.Sender))
        {
            VisibleCellsProperty visibleCellsProperty = playerService.GetProperty<VisibleCellsProperty>(player);
            bool all = true;
            foreach (Entity entity in entities)
            {
                if (!visibleCellsProperty.CanSee(entity))
                {
                    all = false;
                    break;
                }
            }
            if (all)
            {
                await context.SendAsync(packet, player);
            }
        }
    }

    public abstract Task Process(AuthProcessorContext context, T packet);
}
