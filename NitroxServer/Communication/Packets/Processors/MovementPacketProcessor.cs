using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class MovementPacketProcessor : AuthenticatedPacketProcessor<Movement>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityManager entityManager;
        private readonly EntitySimulation entitySimulation;

        public MovementPacketProcessor(EntityManager entityManager, EntitySimulation entitySimulation, PlayerManager playerManager)
        {
            this.entityManager = entityManager;
            this.entitySimulation = entitySimulation;
            this.playerManager = playerManager;
        }

        public override void Process(Movement packet, Player player)
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
            player.Transform.Position = packet.Position;
        }
    }
}
