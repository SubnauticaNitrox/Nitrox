using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
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
