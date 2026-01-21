using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

internal abstract class TransmitIfCanSeePacketProcessor<T>(PlayerManager playerManager, EntityRegistry entityRegistry) : IAuthPacketProcessor<T>
    where T : Packet
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    /// <summary>
    /// Transmits the provided <paramref name="packet"/> to all other players (excluding <paramref name="senderPlayer"/>)
    /// who can see (<see cref="Player.CanSee"/>) entities corresponding to the provided <paramref name="entityIds"/> only if all those entities are registered.
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

        foreach (Player player in playerManager.GetConnectedPlayersExcept(context.Sender))
        {
            if (entities.All(player.CanSee))
            {
                await context.SendAsync(packet, player.SessionId);
            }
        }
    }

    public abstract Task Process(AuthProcessorContext context, T packet);
}
