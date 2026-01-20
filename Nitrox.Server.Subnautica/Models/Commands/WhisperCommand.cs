using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("w", "msg", "m")]
[RequiresPermission(Perms.PLAYER)]
internal class WhisperCommand(IPacketSender packetSender) : ICommandHandler<Player, string>
{
    [Description("Sends a private message to a player")]
    public async Task Execute(ICommandContext context, [Description("The players name to message")] Player targetPlayer, [Description("The message to send")] string message)
    {
        await packetSender.SendPacketAsync(new ChatMessage(context.OriginId, $"[{context.OriginName} -> YOU]: {message}"), targetPlayer.SessionId);
    }
}
