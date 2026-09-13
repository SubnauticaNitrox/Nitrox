using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerDeathEventProcessor(PlayerService playerService, IOptions<SubnauticaServerOptions> config) : IAuthPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> options = config;

    public async Task Process(AuthProcessorContext context, PlayerDeathEvent packet)
    {
        if (options.Value.IsHardcore())
        {
            playerService.GetProperty<IsPermaDeathProperty>(context.Sender).Value = true;
            await context.ReplyAsync(new PlayerKicked("Permanent death from hardcore mode"));
        }
        playerService.GetProperty<CheckpointPositionProperty>(context.Sender).Value = packet.DeathPosition;
        playerService.GetProperty<CheckpointSubRootIdProperty>(context.Sender).Value = playerService.GetProperty<SubRootIdProperty>(context.Sender).Value;
        if (playerService.GetProperty<PermissionsProperty>(context.Sender).Value > Perms.MODERATOR)
        {
            await context.ReplyAsync(new ChatMessage((SessionId)SessionId.SERVER_ID, "You can use /back to go to your death location"));
        }
        await context.SendToOthersAsync(packet);
    }
}
