using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class AutoSaveCommand(IOptions<SubnauticaServerOptions> serverOptionsProvider) : ICommandHandler<bool>
{
    private readonly IOptions<SubnauticaServerOptions> serverOptionsProvider = serverOptionsProvider;

    [Description("Whether autosave should be on or off")]
    public async Task Execute(ICommandContext context, bool toggle)
    {
        if (toggle)
        {
            serverOptionsProvider.Value.AutoSave = false;
            await context.ReplyAsync("Enabled periodical saving");
        }
        else
        {
            serverOptionsProvider.Value.AutoSave = true;
            await context.ReplyAsync("Disabled periodical saving");
        }
    }
}
