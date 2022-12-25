#if DEBUG
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

internal sealed class StartScreen_TryToShowDisclaimer_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StartScreen t) => t.TryToShowDisclaimer());

    /// <summary>
    ///     Speed up startup in development by skipping disclaimer screen.
    /// </summary>
    public static bool Prefix() => false;

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
#endif

