using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("w", "msg", "m")]
[RequiresPermission(Perms.PLAYER)]
internal sealed class WhisperCommand : ICommandHandler<SessionId, string>
{
    [Description("Sends a private message to a player")]
    public async Task Execute(ICommandContext context, [Description("The players name to message")] SessionId targetPlayer, [Description("The message to send")] string message)
    {
        await context.SendAsync(targetPlayer, new ChatMessage(context.OriginId, $"[{context.OriginName} -> YOU]: {message}"));
    }
}
