using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

internal abstract class TransmitIfCanSeePacketProcessor<T> : AuthenticatedPacketProcessor<T> where T : Packet
{
    private readonly IPacketSender packetSender;
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public TransmitIfCanSeePacketProcessor(IPacketSender packetSender, PlayerManager playerManager, EntityRegistry entityRegistry)
    {
        this.packetSender = packetSender;
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
    }

    /// <summary>
    /// Transmits the provided <paramref name="packet"/> to all other players (excluding <paramref name="senderPlayer"/>)
    /// who can see (<see cref="Player.CanSee"/>) entities corresponding to the provided <paramref name="entityIds"/> only if all those entities are registered.
    /// </summary>
    public void TransmitIfCanSeeEntities(Packet packet, Player senderPlayer, List<NitroxId> entityIds)
    {
        List<Entity> entities = [];
        foreach (NitroxId entityId in entityIds)
        {
            if (entityRegistry.TryGetEntityById(entityId, out Entity entity))
            {
                entities.Add(entity);
            }
            else
            {
                return;
            }
        }

        foreach (Player player in playerManager.GetConnectedPlayersExcept(senderPlayer))
        {
            if (entities.All(player.CanSee))
            {
                packetSender.SendPacketAsync(packet, player.SessionId);
            }
        }
    }
}
