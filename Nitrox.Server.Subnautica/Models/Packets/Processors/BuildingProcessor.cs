using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

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
    /// If the attempt is successful, it will be notified to all players.
    /// Otherwise, nothing will happen.
    /// </summary>
    public void TryClaimBuildPiece(Entity entity, Player player)
    {
        if (entitySimulation.TryAssignEntityToPlayer(entity, player, false, out SimulatedEntity simulatedEntity))
        {
            SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
            playerManager.SendPacketToAllPlayers(ownershipChangePacket);
        }
    }
}
