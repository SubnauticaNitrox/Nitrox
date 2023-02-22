using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class EntityDestroyedPacketProcessor : AuthenticatedPacketProcessor<EntityDestroyed>
{
    private readonly PlayerManager playerManager;
    private readonly WorldEntityManager worldEntityManager;

    public EntityDestroyedPacketProcessor(PlayerManager playerManager, WorldEntityManager worldEntityManager)
    {
        this.playerManager = playerManager;
        this.worldEntityManager = worldEntityManager;
    }

    public override void Process(EntityDestroyed packet, Player destroyingPlayer)
    {
        if (worldEntityManager.TryDestroyEntity(packet.Id, out Optional<Entity> entity))
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
