using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("dir")]
[RequiresOrigin(CommandOrigin.SERVER)]
internal sealed class DirectoryCommand(IOptions<ServerStartOptions> optionsProvider, ILogger<DirectoryCommand> logger) : ICommandHandler<DirectoryCommand.CommonDirectory>
{
    private readonly IOptions<ServerStartOptions> optionsProvider = optionsProvider;
    private readonly ILogger<DirectoryCommand> logger = logger;

    [Description("Opens save directory or other directory by name")]
    public Task Execute(ICommandContext context, [Description("Common name of the directory to open")] CommonDirectory commonDirectory = CommonDirectory.SAVE)
    {
        string path = GetPathFromCommonDirectory(commonDirectory);
        if (!Directory.Exists(path))
        {
            logger.ZLogError($"Could not find or access directory {commonDirectory}");
            return Task.CompletedTask;
        }

        logger.ZLogInformation($"Opening directory {path:@Path}");
        using Process proc = Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
        return Task.CompletedTask;
    }

    private string? GetPathFromCommonDirectory(CommonDirectory commonDir = CommonDirectory.SELF) =>
        commonDir switch
        {
            CommonDirectory.SELF => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location),
            CommonDirectory.SAVE => optionsProvider.Value.GetServerSavePath(),
            CommonDirectory.LOG => optionsProvider.Value.GetServerLogsPath(),
            CommonDirectory.GAME => optionsProvider.Value.GamePath,
            _ => throw new ArgumentOutOfRangeException(nameof(commonDir), commonDir, null)
        };

    public enum CommonDirectory
    {
        SELF = 0,
        EXE = 0,
        EXECUTABLE = 0,
        SAVE = 1,
        SAVES = 1,
        LOG = 2,
        LOGS = 2,
        GAME = 3,
        GAMEFILES = 3
    }
}
