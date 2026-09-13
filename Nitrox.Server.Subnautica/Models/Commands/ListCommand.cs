using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal sealed class ListCommand(IOptions<SubnauticaServerOptions> options, PlayerService playerService) : ICommandHandler
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> options = options;

    [Description("Shows who's online")]
    public async Task Execute(ICommandContext context)
    {
        string[] players = GetPlayerListText().ToArray();
        StringBuilder builder = new($"List of players ({players.Length}/{options.Value.MaxConnections}):\n");
        builder.Append(string.Join(", ", players));

        await context.ReplyAsync(builder.ToString());
    }

    private IEnumerable<string> GetPlayerListText()
    {
        foreach (SessionId sessionId in playerService.GetSessionIds())
        {
            yield return $"{playerService.GetProperty<NameProperty>(sessionId).Value} #{sessionId}";
        }
    }
}
