using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntityTransformUpdatesProcessor(PlayerManager playerManager, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData) : IAuthPacketProcessor<EntityTransformUpdates>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, EntityTransformUpdates packet)
    {
        Dictionary<Player, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = InitializeVisibleUpdateMapWithOtherPlayers(context.Sender);
        AssignVisibleUpdatesToPlayers(context.Sender, packet.Updates, visibleUpdatesByPlayer);
        await SendUpdatesToPlayersAsync(context, visibleUpdatesByPlayer);
    }

    private Dictionary<Player, List<EntityTransformUpdates.EntityTransformUpdate>> InitializeVisibleUpdateMapWithOtherPlayers(Player simulatingPlayer)
    {
        Dictionary<Player, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = [];
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            if (!player.Equals(simulatingPlayer))
            {
                visibleUpdatesByPlayer[player] = [];
            }
        }
        return visibleUpdatesByPlayer;
    }

    private void AssignVisibleUpdatesToPlayers(Player sendingPlayer, List<EntityTransformUpdates.EntityTransformUpdate> updates, Dictionary<Player, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (EntityTransformUpdates.EntityTransformUpdate update in updates)
        {
            if (!simulationOwnershipData.TryGetLock(update.Id, out SimulationOwnershipData.PlayerLock playerLock) || playerLock.Player != sendingPlayer)
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

            foreach (KeyValuePair<Player, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
            {
                if (playerUpdates.Key.CanSee(worldEntity))
                {
                    playerUpdates.Value.Add(update);
                }
            }
        }
    }

    private async Task SendUpdatesToPlayersAsync(AuthProcessorContext context, Dictionary<Player, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (KeyValuePair<Player, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
        {
            Player player = playerUpdates.Key;
            List<EntityTransformUpdates.EntityTransformUpdate> updates = playerUpdates.Value;

            if (updates.Count > 0)
            {
                await context.SendAsync(new EntityTransformUpdates(updates), player.SessionId);
            }
        }
    }
}
