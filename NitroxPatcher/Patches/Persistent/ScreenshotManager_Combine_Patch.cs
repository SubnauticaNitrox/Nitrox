using System.IO;
using System.Reflection;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
///     Changes Subnautica screenshot save location to <see cref="NitroxUser.ScreenshotsPath" />.
/// </summary>
public partial class ScreenshotManager_Combine_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => ScreenshotManager.Combine(default(string), default(string)));

    public static bool Prefix(string path1, string path2, ref string? __result)
    {
        __result = path2 switch
        {
            ScreenshotManager.screenshotsFolderName => NitroxUser.ScreenshotsPath,
            { } filename when path1 == ScreenshotManager.savePath => Path.Combine(NitroxUser.ScreenshotsPath, Path.GetFileName(filename)),
            _ => null
        };
        return __result == null; // Run original method if we didn't handle (result is null).
    }
}
