using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class LeakRepairedProcessor : AuthenticatedPacketProcessor<LeakRepaired>
{
    private readonly WorldEntityManager worldEntityManager;
    private readonly PlayerManager playerManager;

    public LeakRepairedProcessor(WorldEntityManager worldEntityManager, PlayerManager playerManager)
    {
        this.worldEntityManager = worldEntityManager;
        this.playerManager = playerManager;
    }

    public override void Process(LeakRepaired packet, Player player)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
