using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("say")]
[RequiresPermission(Perms.MODERATOR)]
internal sealed class BroadcastCommand(ILogger<BroadcastCommand> logger) : ICommandHandler<string>
{
    private readonly ILogger<BroadcastCommand> logger = logger;

    [Description("Broadcasts a message on the server")]
    public async Task Execute(ICommandContext context, string messageToBroadcast)
    {
        await context.SendToAllAsync(messageToBroadcast);
        logger.ZLogInformation($"{context.OriginName} #{context.OriginId:@SessionId} sent a message to everyone: '{messageToBroadcast:@ChatMessage}'");
    }
}
