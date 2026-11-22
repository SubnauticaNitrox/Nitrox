using System.ComponentModel;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("tp")]
[RequiresOrigin(CommandOrigin.PLAYER)]
[RequiresPermission(Perms.MODERATOR)]
internal sealed class TeleportCommand(ITeleport teleporter) : ICommandHandler<int, int, int>
{
    private readonly ITeleport teleporter = teleporter;

    [Description("Teleports you on a specific location")]
    public async Task Execute(ICommandContext context, [Description("x coordinate")] int x, [Description("y coordinate")] int y, [Description("z coordinate")] int z)
    {
        NitroxVector3 position = new(x, y, z);
        await teleporter.TeleportAsync(context.OriginId, position, Optional.Empty);
        await context.ReplyAsync($"Teleported to {position}");
    }
}
