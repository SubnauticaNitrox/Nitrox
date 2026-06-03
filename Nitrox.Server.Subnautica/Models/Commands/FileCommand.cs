using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("log")]
[RequiresOrigin(CommandOrigin.SERVER)]
[Description("Opens the log file that is currently in use")]
internal sealed class FileCommand(IOptions<ServerStartOptions> options, ILogger<FileCommand> logger) : ICommandHandler
{
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly ILogger<FileCommand> logger = logger;

    public Task Execute(ICommandContext context)
    {
        try
        {
            try
            {
                string? lastWriteLogFilePath = Directory.EnumerateFiles(NitroxDirectory.LogsPath, $"*_{options.Value.SaveName}_*").OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(lastWriteLogFilePath))
                {
                    logger.ZLogInformation($"Log file is unavailable");
                    return Task.CompletedTask;
                }
                using Process proc = Process.Start(new ProcessStartInfo
                {
                    FileName = lastWriteLogFilePath,
                    UseShellExecute = true,
                    Verb = "open"
                });
                logger.ZLogInformation($"Opening log file at: {lastWriteLogFilePath:@Path}");
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Failed to open latest log file");
            }
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }
}
