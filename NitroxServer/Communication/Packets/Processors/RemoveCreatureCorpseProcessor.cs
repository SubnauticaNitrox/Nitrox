using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class RemoveCreatureCorpseProcessor : AuthenticatedPacketProcessor<RemoveCreatureCorpse>
{
    private readonly PlayerManager playerManager;
    private readonly EntitySimulation entitySimulation;
    private readonly WorldEntityManager worldEntityManager;

    public RemoveCreatureCorpseProcessor(PlayerManager playerManager, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager)
    {
        this.playerManager = playerManager;
        this.worldEntityManager = worldEntityManager;
        this.entitySimulation = entitySimulation;
    }

    public override void Process(RemoveCreatureCorpse packet, Player destroyingPlayer)
    {
        // TODO: In the future, for more immersion (though that's a neglectable +), have a corpse entity on server-side or a dedicated metadata for this entity (CorpseMetadata)
        // So that even players rejoining can see it (before it despawns)
        entitySimulation.EntityDestroyed(packet.CreatureId);

        if (worldEntityManager.TryDestroyEntity(packet.CreatureId, out Optional<Entity> entity))
        {
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != destroyingPlayer;
                if (isOtherPlayer && player.CanSee(entity.Value))
                {
                    player.SendPacket(packet);
                }
            }
        }
    }
}
