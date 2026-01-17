using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EscapePodChangedPacketProcessor(PlayerManager playerManager, ILogger<EscapePodChangedPacketProcessor> logger) : AuthenticatedPacketProcessor<EscapePodChanged>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly ILogger<EscapePodChangedPacketProcessor> logger = logger;

    public override void Process(EscapePodChanged packet, Player player)
    {
        player.SubRootId = packet.EscapePodId;
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
