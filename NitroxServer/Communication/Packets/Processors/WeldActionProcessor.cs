using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
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
