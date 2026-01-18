using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.HOST)]
internal sealed class ChangeAdminPasswordCommand(IOptions<SubnauticaServerOptions> options, ILogger<ChangeAdminPasswordCommand> logger) : ICommandHandler<string>
{
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<ChangeAdminPasswordCommand> logger = logger;

    [Description("Changes admin password")]
    public async Task Execute(ICommandContext context, string newPassword)
    {
        options.Value.AdminPassword = newPassword;
        logger.ZLogInformation($"Admin password changed to {newPassword:@Password} by {context.OriginName:@Name}");
        await context.ReplyAsync("Admin password has been updated");
    }
}
