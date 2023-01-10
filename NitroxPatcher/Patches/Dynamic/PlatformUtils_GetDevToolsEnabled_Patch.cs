using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

internal sealed class PlatformUtils_GetDevToolsEnabled_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TargetMethod = Reflect.Method(() => PlatformUtils.GetDevToolsEnabled());

    /// <summary>
    ///     Disable dev console if server has disabled cheats.
    /// </summary>
    public static void Postfix(ref bool __result)
    {
        if (NitroxConsole.DisableConsole)
        {
            __result = false;
        }
        else
        {
            // Convenience: enable the console (but don't show yet) so that you don't need to press shift+tilde keys first.
            PlatformUtils.SetDevToolsEnabled(true);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TargetMethod);
    }
}
