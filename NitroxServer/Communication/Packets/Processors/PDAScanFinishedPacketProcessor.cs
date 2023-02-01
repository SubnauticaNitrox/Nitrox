using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors;

public class PDAScanFinishedPacketProcessor : AuthenticatedPacketProcessor<PDAScanFinished>
{
    private readonly PlayerManager playerManager;
    private readonly PDAStateData pdaStateData;
    private readonly EntityRegistry entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData;

    public PDAScanFinishedPacketProcessor(PlayerManager playerManager, PDAStateData pdaStateData, EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData)
    {
        this.playerManager = playerManager;
        this.pdaStateData = pdaStateData;
        this.entityRegistry = entityRegistry;
        this.simulationOwnershipData = simulationOwnershipData;
    }

    public override void Process(PDAScanFinished packet, Player player)
    {
        pdaStateData.FinishScanProgress(packet.Id, packet.TechType, packet.Destroy, packet.FullyResearched);
        pdaStateData.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
        playerManager.SendPacketToOtherPlayers(packet, player);

        if (packet.Destroy)
        {
            entityRegistry.RemoveEntity(packet.Id);
            if (simulationOwnershipData.RevokeOwnerOfId(packet.Id))
            {
                playerManager.SendPacketToAllPlayers(new SimulationOwnershipChange(packet.Id, ushort.MaxValue, SimulationLockType.TRANSIENT));
            }
        }
    }
}
