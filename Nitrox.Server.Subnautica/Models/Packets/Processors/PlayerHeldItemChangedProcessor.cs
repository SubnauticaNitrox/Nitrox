using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerHeldItemChangedProcessor() : IAuthPacketProcessor<PlayerHeldItemChanged>
{
    public async Task Process(AuthProcessorContext context, PlayerHeldItemChanged packet)
    {
        // TODO: USE DATABASE
        // if (packet.IsFirstTime != null && !player.UsedItems.Contains(packet.IsFirstTime))
        // {
        //     player.UsedItems.Add(packet.IsFirstTime);
        // }
        //
        // playerService.SendPacketToOtherPlayers(packet, player);
    }
}
