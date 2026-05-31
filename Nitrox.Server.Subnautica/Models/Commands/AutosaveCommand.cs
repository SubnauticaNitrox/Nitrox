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
        SubnauticaServerOptions options = serverOptionsProvider.Value;
        if (toggle)
        {
            // Ensure save interval is a sensible value before turning on auto saving.
            if (options.SaveInterval <= 1000)
            {
                options.SaveInterval = new SubnauticaServerOptions().SaveInterval;
            }
            options.AutoSave = true;
            await context.ReplyAsync("Enabled periodical saving");
        }
        else
        {
            options.AutoSave = false;
            await context.ReplyAsync("Disabled periodical saving");
        }
    }
}
