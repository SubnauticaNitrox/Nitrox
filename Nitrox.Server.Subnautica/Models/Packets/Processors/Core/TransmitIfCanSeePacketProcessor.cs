using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

public abstract class TransmitIfCanSeePacketProcessor<T> : AuthenticatedPacketProcessor<T> where T : Packet
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public TransmitIfCanSeePacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
    {
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
                player.SendPacket(packet);
            }
        }
    }
}
