using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RemoveCreatureCorpseProcessor(IPacketSender packetSender, PlayerManager playerManager, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : AuthenticatedPacketProcessor<RemoveCreatureCorpse>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

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
                    packetSender.SendPacketAsync(packet, player.SessionId);
                }
            }
        }
    }
}
