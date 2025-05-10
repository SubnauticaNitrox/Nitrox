using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("say")]
[RequiresPermission(Perms.MODERATOR)]
internal sealed class BroadcastCommand : ICommandHandler<string>
{
    [Description("Broadcasts a message on the server")]
    public async Task Execute(ICommandContext context, string messageToBroadcast)
    {
        await context.MessageAllAsync(messageToBroadcast);
    }
}
