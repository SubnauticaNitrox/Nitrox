using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

// TODO: This command might not be necessary when using SQLite database as it'll save itself.
[RequiresPermission(Perms.MODERATOR)]
internal class SaveCommand : ICommandHandler
{
    [Description("Saves the world")]
    public Task Execute(ICommandContext context)
    {
        context.MessageAllAsync("World is saving...");

        // TODO: Run save action on server (run via command?)
        // persistenceService.SaveAsync() (allow async command execute)
        // NitroxServer.Server.Instance.Save();

        return Task.CompletedTask;
    }
}
