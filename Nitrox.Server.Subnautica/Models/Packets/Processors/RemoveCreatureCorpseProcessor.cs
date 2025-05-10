using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class RemoveCreatureCorpseProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<RemoveCreatureCorpse>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext destroyingPlayer, RemoveCreatureCorpse packet)
    {
        // TODO: In the future, for more immersion (though that's a neglectable +), have a corpse entity on server-side or a dedicated metadata for this entity (CorpseMetadata)
        // So that even players rejoining can see it (before it despawns)
        entitySimulation.EntityDestroyed(packet.CreatureId);

        if (worldEntityManager.TryDestroyEntity(packet.CreatureId, out Entity entity))
        {
            // TODO: USE DATABASE
            // foreach (PeerId player in playerManager.GetConnectedPlayersAsync())
            // {
            //     bool isOtherPlayer = player != destroyingPlayer.Sender;
            //     if (isOtherPlayer && player.CanSee(entity))
            //     {
            //         player.SendPacket(packet);
            //     }
            // }
        }
    }
}
