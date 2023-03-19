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
    class CellVisibilityChangedProcessor : AuthenticatedPacketProcessor<CellVisibilityChanged>
    {
        private readonly WorldEntityManager worldEntityManager;
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public CellVisibilityChangedProcessor(EntitySimulation entitySimulation, PlayerManager playerManager)
        {
            this.entitySimulation = entitySimulation;
            this.playerManager = playerManager;
        }

        public override void Process(CellVisibilityChanged packet, Player player)
        {
            player.AddCells(packet.Added);
            player.RemoveCells(packet.Removed);

            entitySimulation.CalculateSimulationChangesFromCellSwitch(player, packet.Added, packet.Removed);
        }
    }
}
