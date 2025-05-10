using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityTransformUpdatesProcessor(WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, IServerPacketSender packetSender) : IAuthPacketProcessor<EntityTransformUpdates>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly IServerPacketSender packetSender = packetSender;

    public async Task Process(AuthProcessorContext context, EntityTransformUpdates packet)
    {
        Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = InitializeVisibleUpdateMapWithOtherPlayers(context.Sender.SessionId);
        AssignVisibleUpdatesToPlayers(context.Sender.SessionId, packet.Updates, visibleUpdatesByPlayer);
        await SendUpdatesToPlayers(visibleUpdatesByPlayer);
    }

    private Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> InitializeVisibleUpdateMapWithOtherPlayers(SessionId simulatingPlayer)
    {
        Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = new();

        // TODO: Fix using database
        // foreach (PeerId player in playerManager.GetConnectedPlayersAsync())
        // {
        //     if (!player.Equals(simulatingPlayer))
        //     {
        //         visibleUpdatesByPlayer[player] = new List<EntityTransformUpdates.EntityTransformUpdate>();
        //     }
        // }

        return visibleUpdatesByPlayer;
    }

    private void AssignVisibleUpdatesToPlayers(SessionId sendingPlayer, List<EntityTransformUpdates.EntityTransformUpdate> updates, Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        // TODO: USE DATABASE
        // foreach (EntityTransformUpdates.EntityTransformUpdate update in updates)
        // {
        //     if (!simulationOwnershipData.TryGetLock(update.Id, out SimulationOwnershipData.PlayerLock playerLock) || playerLock.Player != sendingPlayer)
        //     {
        //         // This will happen pretty frequently when a player moves very fast (swimfast or maybe some more edge cases) so we can just ignore this
        //         continue;
        //     }
        //
        //     if (!worldEntityManager.TryUpdateEntityPosition(update.Id, update.Position, update.Rotation, out AbsoluteEntityCell currentCell, out WorldEntity worldEntity))
        //     {
        //         // Normal behaviour if the entity was removed at the same time as someone trying to simulate a postion update.
        //         // we log an info inside entityManager.UpdateEntityPosition just in case.
        //         continue;
        //     }
        //
        //     foreach (KeyValuePair<NitroxServer.Player, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
        //     {
        //         if (playerUpdates.Key.CanSee(worldEntity))
        //         {
        //             playerUpdates.Value.Add(update);
        //         }
        //     }
        // }
    }

    private async Task SendUpdatesToPlayers(Dictionary<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (KeyValuePair<SessionId, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
        {
            SessionId player = playerUpdates.Key;
            List<EntityTransformUpdates.EntityTransformUpdate> updates = playerUpdates.Value;

            if (updates.Count > 0)
            {
                EntityTransformUpdates updatesPacket = new(updates);
                await packetSender.SendPacket(updatesPacket, player);
            }
        }
    }
}
