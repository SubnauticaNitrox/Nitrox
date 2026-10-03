using System.ComponentModel;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class ChangeServerGamemodeCommand(PlayerService playerService, IOptions<SubnauticaServerOptions> serverConfig) : ICommandHandler<SubnauticaGameMode>
{
    private readonly IOptions<SubnauticaServerOptions> serverConfig = serverConfig;
    private readonly PlayerService playerService = playerService;

    [Description("Changes server gamemode")]
    public async Task Execute(ICommandContext context, [Description("Gamemode to change to")] SubnauticaGameMode newGameMode)
    {
        if (serverConfig.Value.GameMode == newGameMode)
        {
            await context.ReplyAsync("Server is already using this gamemode");
            return;
        }

        serverConfig.Value.GameMode = newGameMode;
        foreach ((SessionId _, GameModeProperty property) in playerService.GetProperties<GameModeProperty>())
        {
            property.Value = newGameMode;
        }
        await context.SendToAllAsync(GameModeChanged.ForAllPlayers(newGameMode));
        await context.SendToAllAsync($"Server gamemode changed to \"{newGameMode}\" by {context.OriginName}");
    }
}
