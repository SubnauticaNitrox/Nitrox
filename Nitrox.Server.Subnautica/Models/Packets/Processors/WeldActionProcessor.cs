using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WeldActionProcessor : AuthenticatedPacketProcessor<WeldAction>
{
    private readonly IPacketSender packetSender;
    private readonly SimulationOwnershipData simulationOwnershipData;
    private readonly ILogger<WeldActionProcessor> logger;

    public WeldActionProcessor(IPacketSender packetSender, SimulationOwnershipData simulationOwnershipData, ILogger<WeldActionProcessor> logger)
    {
        this.packetSender = packetSender;
        this.simulationOwnershipData = simulationOwnershipData;
        this.logger = logger;
    }

    public override void Process(WeldAction packet, Player player)
    {
        Player simulatingPlayer = simulationOwnershipData.GetPlayerForLock(packet.Id);

        if (simulatingPlayer != null)
        {
            logger.ZLogDebug($"Send WeldAction to simulating player {simulatingPlayer.Name} for entity {packet.Id}");
            packetSender.SendPacketAsync(packet, simulatingPlayer.SessionId);
        }
    }
}
