using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using static NitroxModel.Packets.EntityTransformUpdates;

namespace NitroxServer.Communication.Packets.Processors
{
    class EntityTransformUpdatesProcessor : AuthenticatedPacketProcessor<EntityTransformUpdates>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityManager entityManager;

        public EntityTransformUpdatesProcessor(PlayerManager playerManager, EntityManager entityManager)
        {
            this.playerManager = playerManager;
            this.entityManager = entityManager;
        }

        public override void Process(EntityTransformUpdates packet, Player simulatingPlayer)
        {
            Dictionary<Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer = InitializeVisibleUpdateMapWithOtherPlayers(simulatingPlayer);
            AssignVisibleUpdatesToPlayers(packet.Updates, visibleUpdatesByPlayer);
            SendUpdatesToPlayers(visibleUpdatesByPlayer);
        }

        private Dictionary<Player, List<EntityTransformUpdate>> InitializeVisibleUpdateMapWithOtherPlayers(Player simulatingPlayer)
        {
            Dictionary<Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer = new Dictionary<Player, List<EntityTransformUpdate>>();

            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                if (!player.Equals(simulatingPlayer))
                {
                    visibleUpdatesByPlayer[player] = new List<EntityTransformUpdate>();
                }
            }

            return visibleUpdatesByPlayer;
        }

        private void AssignVisibleUpdatesToPlayers(List<EntityTransformUpdate> updates, Dictionary<Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer)
        {
            foreach (EntityTransformUpdate update in updates)
            {
                Optional<AbsoluteEntityCell> currentCell = entityManager.UpdateEntityPosition(update.Id, update.Position, update.Rotation);

                if(!currentCell.HasValue)
                {
                    // Normal behaviour if the entity was removed at the same time as someone trying to simulate a postion update.
                    // we log an info inside entityManager.UpdateEntityPosition just in case.
                    continue;
                }

                foreach (KeyValuePair<Player, List<EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
                {
                    Player player = playerUpdates.Key;
                    List<EntityTransformUpdate> visibleUpdates = playerUpdates.Value;

                    if (player.HasCellLoaded(currentCell.Value))
                    {
                        visibleUpdates.Add(update);
                    }
                }
            }
        }

        private void SendUpdatesToPlayers(Dictionary<Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer)
        {
            foreach (KeyValuePair<Player, List<EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
            {
                Player player = playerUpdates.Key;
                List<EntityTransformUpdate> updates = playerUpdates.Value;

                if (updates.Count > 0)
                {
                    EntityTransformUpdates updatesPacket = new EntityTransformUpdates(updates);
                    player.SendPacket(updatesPacket);
                }
            }
        }
    }
}
