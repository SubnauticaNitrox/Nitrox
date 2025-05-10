using System;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

internal abstract class TransmitIfCanSeePacketProcessor<T>(GameLogic.EntityRegistry entityRegistry) : IAuthPacketProcessor<T>
    where T : Packet
{
    private readonly GameLogic.EntityRegistry entityRegistry = entityRegistry;

    /// <summary>
    ///     Transmits the provided <paramref name="packet" /> to all other players (excluding <paramref name="senderPlayer" />)
    ///     who can see (<see cref="NitroxServer.Player.CanSee" />) entities corresponding to the provided
    ///     <paramref name="entityIds" /> only if all those entities are registered.
    /// </summary>
    protected Task TransmitIfCanSeeEntities(Packet packet, PeerId senderPlayer, params Span<NitroxId> entityIds)
    {
        // TODO: LET DATABASE DO THIS CHECK (AS A QUERY)

        // Entity[] entities = ArrayPool<Entity>.Shared.Rent(entityIds.Length);
        // try
        // {
        //     int knownEntityCount = 0;
        //     foreach (NitroxId entityId in entityIds)
        //     {
        //         if (entityRegistry.TryGetEntityById(entityId, out Entity entity))
        //         {
        //             entities[knownEntityCount++] = entity;
        //         }
        //         else
        //         {
        //             return;
        //         }
        //     }
        //
        //     foreach (NitroxServer.Player player in playerService.GetConnectedPlayersExcept(senderPlayer))
        //     {
        //         bool canSeeAll = true;
        //         foreach (Entity entity in entities.AsSpan()[..knownEntityCount])
        //         {
        //             if (!player.CanSee(entity))
        //             {
        //                 canSeeAll = false;
        //                 break;
        //             }
        //         }
        //
        //         if (canSeeAll)
        //         {
        //             player.SendPacket(packet);
        //         }
        //     }
        // }
        // finally
        // {
        //     ArrayPool<Entity>.Shared.Return(entities);
        // }

        return Task.CompletedTask;
    }

    public abstract Task Process(AuthProcessorContext context, T packet);
}
