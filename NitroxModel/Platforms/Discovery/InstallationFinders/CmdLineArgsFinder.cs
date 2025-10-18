using System.IO;
using System.Linq;
using NitroxModel.Helper;
using NitroxModel.Platforms.Discovery.InstallationFinders.Core;
using static NitroxModel.Platforms.Discovery.InstallationFinders.Core.GameFinderResult;

namespace NitroxModel.Platforms.Discovery.InstallationFinders;

/// <summary>
///     Tries to get the --game-path command line argument which contains the installation directory.
/// </summary>
public sealed class CmdLineArgsFinder : IGameFinder
{
    public GameFinderResult FindGame(GameInfo gameInfo)
    {
        string? cliGamePath = NitroxEnvironment.CommandLineArgs.GetCommandArgs("--game-path").FirstOrDefault();
        if (string.IsNullOrEmpty(cliGamePath))
        {
            return NotFound();
        }

        if (Directory.Exists(cliGamePath) && Path.IsPathRooted(cliGamePath))
        {
            return Ok(cliGamePath);
        }

        return Error($"Game directory not found at user-specified location: {cliGamePath}");
    }
}
