using System.ComponentModel;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("tp")]
[RequiresOrigin(CommandOrigin.PLAYER)]
[RequiresPermission(Perms.MODERATOR)]
internal sealed class TeleportCommand : ICommandHandler<int, int, int>
{
    [Description("Teleports you on a specific location")]
    public async Task Execute(ICommandContext context, [Description("x coordinate")] int x, [Description("y coordinate")] int y, [Description("z coordinate")] int z)
    {
        if (context is not PlayerToServerCommandContext playerContext)
        {
            return;
        }

        NitroxVector3 position = new(x, y, z);
        playerContext.Player.Teleport(position, Optional.Empty);
        await playerContext.ReplyAsync($"Teleported to {position}");
    }
}
