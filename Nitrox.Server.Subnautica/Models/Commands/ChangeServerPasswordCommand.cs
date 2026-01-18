using System.ComponentModel;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal sealed class ChangeServerPasswordCommand(ILogger<ChangeServerPasswordCommand> logger, IOptions<SubnauticaServerOptions> serverConfig) : ICommandHandler, ICommandHandler<string>
{
    private readonly IOptions<SubnauticaServerOptions> serverConfig = serverConfig;
    private readonly ILogger<ChangeServerPasswordCommand> logger = logger;

    [Description("Changes server password. Clear it without argument")]
    public async Task Execute(ICommandContext context, [Description("The new server password")] string newPassword) => await SetPasswordAsync(context, newPassword);

    [Description("Clears server password")]
    public async Task Execute(ICommandContext context) => await SetPasswordAsync(context, "");

    private async Task SetPasswordAsync(ICommandContext context, string password)
    {
        serverConfig.Value.ServerPassword = password;
        logger.LogServerPasswordChanged(password, context.OriginName);
        await context.ReplyAsync("Server password has been updated");
    }
}
