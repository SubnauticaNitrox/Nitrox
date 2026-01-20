using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerHeldItemChangedProcessor(IPacketSender packetSender) : AuthenticatedPacketProcessor<PlayerHeldItemChanged>
{
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(PlayerHeldItemChanged packet, Player player)
    {
        if (packet.IsFirstTime != null && !player.UsedItems.Contains(packet.IsFirstTime))
        {
            player.UsedItems.Add(packet.IsFirstTime);
        }

        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
