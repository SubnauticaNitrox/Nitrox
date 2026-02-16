using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class BackCommand(IPacketSender packetSender, PlayerManager playerManager, ILogger<BackCommand> logger) : ICommandHandler
{
    private readonly ILogger<BackCommand> logger = logger;
    private readonly PlayerManager playerManager = playerManager;
    private readonly IPacketSender packetSender = packetSender;

    [Description("Teleports you back on your last location")]
    public Task Execute(ICommandContext context)
    {
        if (!playerManager.TryGetPlayerBySessionId(context.OriginId, out Player player))
        {
            logger.ZLogError($"Failed to get player instance from session #{context.OriginId}");
            return Task.CompletedTask;
        }

        if (player.LastStoredPosition == null)
        {
            context.ReplyAsync("No previous location...");
            return Task.CompletedTask;
        }

        player.Teleport(player.LastStoredPosition.Value, player.LastStoredSubRootID, packetSender);
        context.ReplyAsync($"Teleported back to {player.LastStoredPosition.Value}");
        return Task.CompletedTask;
    }
}
