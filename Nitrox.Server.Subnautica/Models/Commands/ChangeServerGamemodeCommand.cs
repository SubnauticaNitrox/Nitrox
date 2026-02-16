using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class ChangeServerGamemodeCommand(PlayerManager playerManager, IOptions<SubnauticaServerOptions> serverConfig) : ICommandHandler<SubnauticaGameMode>
{
    private readonly IOptions<SubnauticaServerOptions> serverConfig = serverConfig;
    private readonly PlayerManager playerManager = playerManager;

    [Description("Changes server gamemode")]
    public async Task Execute(ICommandContext context, [Description("Gamemode to change to")] SubnauticaGameMode newGameMode)
    {
        if (serverConfig.Value.GameMode == newGameMode)
        {
            await context.ReplyAsync("Server is already using this gamemode");
            return;
        }

        serverConfig.Value.GameMode = newGameMode;
        foreach (Player player in playerManager.GetAllPlayers())
        {
            player.GameMode = newGameMode;
        }
        await context.SendToAllAsync(GameModeChanged.ForAllPlayers(newGameMode));
        await context.SendToAllAsync($"Server gamemode changed to \"{newGameMode}\" by {context.OriginName}");
    }
}
