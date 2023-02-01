using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors;

public class PDAScanProgressPacketProcessor : AuthenticatedPacketProcessor<PDAScanProgress>
{
    private readonly PlayerManager playerManager;
    private readonly PDAStateData pdaStateData;
    private readonly EntityRegistry entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData;

    public PDAScanProgressPacketProcessor(PlayerManager playerManager, PDAStateData pdaStateData, EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData)
    {
        this.playerManager = playerManager;
        this.pdaStateData = pdaStateData;
        this.entityRegistry = entityRegistry;
        this.simulationOwnershipData = simulationOwnershipData;
    }

    public override void Process(PDAScanProgress packet, Player player)
    {
        if (pdaStateData.UpdateScanProgress(packet.Id, packet.TechType, packet.Progress))
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }

        if (packet.Destroy)
        {
            pdaStateData.FinishScanProgress(packet.Id, packet.TechType, packet.Destroy, false);
            entityRegistry.RemoveEntity(packet.Id);
            if (simulationOwnershipData.RevokeOwnerOfId(packet.Id))
            {
                playerManager.SendPacketToAllPlayers(new SimulationOwnershipChange(packet.Id, ushort.MaxValue, SimulationLockType.TRANSIENT));
            }
        }
    }
}
