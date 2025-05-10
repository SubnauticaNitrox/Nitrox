using System.ComponentModel;
using System.IO;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.SERVER)]
internal sealed class ConfigCommand(IOptions<ServerStartOptions> optionsProvider) : ICommandHandler
{
    private readonly IOptions<ServerStartOptions> optionsProvider = optionsProvider;

    [Description("Opens the server configuration file")]
    public async Task Execute(ICommandContext context)
    {
        string filePath = optionsProvider.Value.GetServerConfigFilePath();
        if (!File.Exists(filePath))
        {
            // TODO: Handle this case to generate config?
            await context.ReplyAsync("No configuration file exists");
        }

        FileSystem.Instance.OpenOrExecuteFile(filePath);
    }
}
