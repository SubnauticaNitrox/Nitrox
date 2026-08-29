using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.SERVER)]
[Description("Shows IP addresses that this server is listening on")]
internal sealed class IpCommand(StatusService statusService) : ICommandHandler
{
    private readonly StatusService statusService = statusService;

    public async Task Execute(ICommandContext context)
    {
        await statusService.LogIpsAsync();
    }
}
