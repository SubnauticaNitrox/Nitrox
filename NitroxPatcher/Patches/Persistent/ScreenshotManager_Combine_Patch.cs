using System.IO;
using System.Reflection;

namespace NitroxPatcher.Patches.Persistent;

public partial class ScreenshotManager_Combine_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => ScreenshotManager.Combine(default(string), default(string)));

    public static bool Prefix(ref string? __result, string path1, string path2)
    {
        __result = (path1, path2) switch
        {
            (_, ScreenshotManager.screenshotsFolderName) => NitroxUser.ScreenshotsPath,
            (_, {} filename) when path1 == ScreenshotManager.savePath => Path.Combine(NitroxUser.ScreenshotsPath, Path.GetFileName(filename)),
            _ => null
        };
        return __result == null; // Run original method if we didn't handle (result is null).
    }
}
