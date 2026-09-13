using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntityTransformUpdatesProcessor(PlayerService playerService, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData) : IAuthPacketProcessor<EntityTransformUpdates>
{
    private readonly PlayerService playerService = playerService;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, EntityTransformUpdates packet)
    {
        Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = InitializeVisibleUpdateMapWithOtherPlayers(context.Sender);
        AssignVisibleUpdatesToPlayers(context.Sender, packet.Updates, visibleUpdatesByPlayer);
        await SendUpdatesToPlayersAsync(context, visibleUpdatesByPlayer);
    }

    private Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> InitializeVisibleUpdateMapWithOtherPlayers(SessionId simulatingPlayer)
    {
        Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = [];
        foreach (SessionId player in playerService.GetSessionIds())
        {
            if (!player.Equals(simulatingPlayer))
            {
                visibleUpdatesByPlayer[player] = [];
            }
        }
        return visibleUpdatesByPlayer;
    }

    private void AssignVisibleUpdatesToPlayers(SessionId sendingPlayer, List<EntityTransformUpdates.EntityTransformUpdate> updates, Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (EntityTransformUpdates.EntityTransformUpdate update in updates)
        {
            if (!simulationOwnershipData.TryGetLock(update.Id, out SimulationOwnershipData.PlayerLock playerLock) || playerLock.SessionId != sendingPlayer)
            {
                // This will happen pretty frequently when a player moves very fast (swimfast or maybe some more edge cases) so we can just ignore this
                continue;
            }

            if (!worldEntityManager.TryUpdateEntityPosition(update.Id, update.Position, update.Rotation, out AbsoluteEntityCell currentCell, out WorldEntity worldEntity))
            {
                // Normal behaviour if the entity was removed at the same time as someone trying to simulate a postion update.
                // we log an info inside entityManager.UpdateEntityPosition just in case.
                continue;
            }

            foreach (KeyValuePair<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
            {
                if (playerService.GetProperty<VisibleCellsProperty>(playerUpdates.Key).CanSee(worldEntity))
                {
                    playerUpdates.Value.Add(update);
                }
            }
        }
    }

    private async Task SendUpdatesToPlayersAsync(AuthProcessorContext context, Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (KeyValuePair<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
        {
            SessionId player = playerUpdates.Key;
            List<EntityTransformUpdates.EntityTransformUpdate> updates = playerUpdates.Value;

            if (updates.Count > 0)
            {
                await context.SendAsync(new EntityTransformUpdates(updates), player);
            }
        }
    }
}
