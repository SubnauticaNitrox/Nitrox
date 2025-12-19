using System.IO;
using System.Threading.Tasks;
using Nitrox.Model.Helper;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Model.Platforms.Store;

public static class Standalone
{
    public static Task<ProcessEx> StartGameAsync(string pathToGameExe, string launchArguments)
    {
        return Task.FromResult(
            ProcessEx.Start(
                pathToGameExe,
                [(NitroxUser.LAUNCHER_PATH_ENV_KEY, NitroxUser.LauncherPath)],
                Path.GetDirectoryName(pathToGameExe),
                launchArguments
            )
        );
    }
}
