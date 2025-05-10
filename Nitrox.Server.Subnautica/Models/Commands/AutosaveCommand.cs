using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class AutoSaveCommand(IOptions<SubnauticaServerConfig> serverOptionsProvider) : ICommandHandler<bool>
{
    private readonly IOptions<SubnauticaServerConfig> serverOptionsProvider = serverOptionsProvider;

    [Description("Whether autosave should be on or off")]
    public async Task Execute(ICommandContext context, bool toggle)
    {
        if (toggle)
        {
            serverOptionsProvider.Value.DisableAutoSave = false;
            await context.ReplyAsync("Enabled periodical saving");
        }
        else
        {
            serverOptionsProvider.Value.DisableAutoSave = true;
            await context.ReplyAsync("Disabled periodical saving");
        }
    }
}
