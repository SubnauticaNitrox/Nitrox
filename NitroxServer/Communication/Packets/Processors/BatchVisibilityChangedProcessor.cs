using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class BatchVisibilityChangedProcessor : AuthenticatedPacketProcessor<BatchVisibilityChanged>
    {
        private readonly WorldEntityManager worldEntityManager;

        public BatchVisibilityChangedProcessor(WorldEntityManager worldEntityManager)
        {
            this.worldEntityManager = worldEntityManager;
        }

        public override void Process(BatchVisibilityChanged packet, Player player)
        {
            SendNewlyVisibleEntities(player, packet.Added);
        }

        private void SendNewlyVisibleEntities(Player player, NitroxInt3[] visibleBatches)
        {
            foreach (NitroxInt3 batch in visibleBatches)
            {
                List<WorldEntity> newlyVisibleEntities = worldEntityManager.GetVisibleEntities(batch);

                if (newlyVisibleEntities.Count > 0)
                {
                    BatchEntities batchEntities = new BatchEntities(newlyVisibleEntities.Cast<Entity>().ToList());
                    player.SendPacket(batchEntities);
                }
            }
        }
    }
}
