using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerDeathEventProcessor : AuthenticatedPacketProcessor<PlayerDeathEvent>
{
    private readonly IPacketSender packetSender;
    private readonly IOptions<SubnauticaServerOptions> options;

    public PlayerDeathEventProcessor(IPacketSender packetSender, IOptions<SubnauticaServerOptions> config)
    {
        this.packetSender = packetSender;
        options = config;
    }

    public override void Process(PlayerDeathEvent packet, Player player)
    {
        if (options.Value.IsHardcore())
        {
            player.IsPermaDeath = true;
            packetSender.SendPacketAsync(new PlayerKicked("Permanent death from hardcore mode"), player.SessionId);
        }
        player.LastStoredPosition = packet.DeathPosition;
        player.LastStoredSubRootID = player.SubRootId;
        if (player.Permissions > Perms.MODERATOR)
        {
            packetSender.SendPacketAsync(new ChatMessage(ChatMessage.SERVER_ID, "You can use /back to go to your death location"), player.SessionId);
        }
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
