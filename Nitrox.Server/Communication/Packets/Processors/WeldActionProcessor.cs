using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Communication.Packets.Processors
{
    class WeldActionProcessor : AuthenticatedPacketProcessor<WeldAction>
    {
        private readonly SimulationOwnershipData simulationOwnershipData;

        public WeldActionProcessor(SimulationOwnershipData simulationOwnershipData)
        {
            this.simulationOwnershipData = simulationOwnershipData;
        }

        public override void Process(WeldAction packet, Player player)
        {
            Player simulatingPlayer = simulationOwnershipData.GetPlayerForLock(packet.Id);

            if (simulatingPlayer != null)
            {
                Log.Debug($"Send WeldAction to simulating player {simulatingPlayer.Name} for entity {packet.Id}");
                simulatingPlayer.SendPacket(packet);
            }
        }
    }
}
