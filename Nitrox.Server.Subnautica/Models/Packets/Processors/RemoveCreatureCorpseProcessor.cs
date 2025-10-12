using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

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
        entitySimulation.EntityDestroyed(packet.CreatureId);

        if (worldEntityManager.TryDestroyEntity(packet.CreatureId, out Entity entity))
        {
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != destroyingPlayer;
                if (isOtherPlayer && player.CanSee(entity))
                {
                    player.OutOfCellVisibleEntities.Remove(entity.Id);
                    player.SendPacket(packet);
                }
            }
        }
    }
}
