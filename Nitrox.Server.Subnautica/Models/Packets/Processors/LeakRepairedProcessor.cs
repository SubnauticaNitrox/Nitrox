using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class LeakRepairedProcessor(WorldEntityManager worldEntityManager, IPacketSender packetSender) : AuthenticatedPacketProcessor<LeakRepaired>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(LeakRepaired packet, Player player)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            packetSender.SendPacketToOthersAsync(packet, player.SessionId);
        }
    }
}
