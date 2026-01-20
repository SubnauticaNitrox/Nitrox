using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal abstract class BuildingProcessor<T>(BuildingManager buildingManager, IPacketSender packetSender, EntitySimulation? entitySimulation = null) : AuthenticatedPacketProcessor<T>
    where T : Packet
{
    protected readonly BuildingManager BuildingManager = buildingManager;
    protected readonly IPacketSender PacketSender = packetSender;
    private readonly EntitySimulation? entitySimulation = entitySimulation;

    public void SendToOtherPlayersWithOperationId(T packet, Player player, int operationId)
    {
        if (packet is OrderedBuildPacket buildPacket)
        {
            buildPacket.OperationId = operationId;
        }
        PacketSender.SendPacketToOthersAsync(packet, player.SessionId);
    }

    /// <summary>
    /// Attempts to acquire simulation ownership on <paramref name="entity"/> for <paramref name="player"/>.
    /// If the attempt is successful, it will be notified to all players.
    /// Otherwise, nothing will happen.
    /// </summary>
    public void TryClaimBuildPiece(Entity entity, Player player)
    {
        ArgumentNullException.ThrowIfNull(entitySimulation);
        if (entitySimulation.TryAssignEntityToPlayer(entity, player, false, out SimulatedEntity simulatedEntity))
        {
            SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
            PacketSender.SendPacketToAllAsync(ownershipChangePacket);
        }
    }
}
