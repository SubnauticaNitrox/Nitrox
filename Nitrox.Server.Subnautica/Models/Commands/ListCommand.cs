using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal sealed class ListCommand(IOptions<SubnauticaServerOptions> options, PlayerManager playerManager) : ICommandHandler
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly IOptions<SubnauticaServerOptions> options = options;

    [Description("Shows who's online")]
    public async Task Execute(ICommandContext context)
    {
        IList<string> players = playerManager.GetConnectedPlayers().Select(player => player.Name).ToList();

        StringBuilder builder = new($"List of players ({players.Count}/{options.Value.MaxConnections}):\n");
        builder.Append(string.Join(", ", players));

        await context.ReplyAsync(builder.ToString());
    }
}
