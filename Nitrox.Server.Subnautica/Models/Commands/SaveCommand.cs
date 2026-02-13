using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class SaveCommand(SaveService saveService, ILogger<SaveCommand> logger) : ICommandHandler
{
    private readonly SaveService saveService = saveService;
    private readonly ILogger<SaveCommand> logger = logger;

    [Description("Saves the map")]
    public async Task Execute(ICommandContext context)
    {
        logger.LogSaveRequest(context.OriginName, context.OriginId);
        await saveService.QueueActionAsync(SaveService.ServiceAction.SAVE);
    }
}
