using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public abstract class BuildingProcessor<T> : AuthenticatedPacketProcessor<T> where T : Packet
{
    internal readonly BuildingManager buildingManager;
    internal readonly PlayerManager playerManager;
    internal readonly EntitySimulation entitySimulation;

    public BuildingProcessor(BuildingManager buildingManager, PlayerManager playerManager, EntitySimulation entitySimulation = null)
    {
        this.buildingManager = buildingManager;
        this.playerManager = playerManager;
        this.entitySimulation = entitySimulation;
    }

    public void SendToOtherPlayersWithOperationId(T packet, Player player, int operationId)
    {
        if (packet is OrderedBuildPacket buildPacket)
        {
            buildPacket.OperationId = operationId;
        }
        playerManager.SendPacketToOtherPlayers(packet, player);
    }

    /// <summary>
    /// Attempts to acquire simulation ownership on <paramref name="entity"/> for <paramref name="player"/>.
    /// If the attempt is unsuccessful, nothing will happen.
    /// </summary>
    public void ClaimBuildPiece(Entity entity, Player player)
    {
        try
        {
            SimulatedEntity simulatedEntity = entitySimulation.AssignNewEntityToPlayer(entity, player, false);
            SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
            playerManager.SendPacketToAllPlayers(ownershipChangePacket);
        }
        catch (Exception)
        { }
    }
}
