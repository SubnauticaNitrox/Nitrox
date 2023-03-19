using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class CellVisibilityChangedProcessor : AuthenticatedPacketProcessor<CellVisibilityChanged>
    {
        private readonly EntitySimulation entitySimulation;

        public CellVisibilityChangedProcessor(EntitySimulation entitySimulation)
        {
            this.entitySimulation = entitySimulation;
        }

        public override void Process(CellVisibilityChanged packet, Player player)
        {
            player.AddCells(packet.Added);
            player.RemoveCells(packet.Removed);

            entitySimulation.BroadcastSimulationChangesForCellUpdates(player, packet.Added, packet.Removed);
        }
    }
}
