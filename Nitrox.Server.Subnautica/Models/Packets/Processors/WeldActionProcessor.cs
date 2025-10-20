using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    class WeldActionProcessor : AuthenticatedPacketProcessor<WeldAction>
    {
        private readonly SimulationOwnershipData simulationOwnershipData;
        private readonly ILogger<WeldActionProcessor> logger;

        public WeldActionProcessor(SimulationOwnershipData simulationOwnershipData, ILogger<WeldActionProcessor> logger)
        {
            this.simulationOwnershipData = simulationOwnershipData;
            this.logger = logger;
        }

        public override void Process(WeldAction packet, Player player)
        {
            Player simulatingPlayer = simulationOwnershipData.GetPlayerForLock(packet.Id);

            if (simulatingPlayer != null)
            {
                logger.ZLogDebug($"Send WeldAction to simulating player {simulatingPlayer.Name} for entity {packet.Id}");
                simulatingPlayer.SendPacket(packet);
            }
        }
    }
}
