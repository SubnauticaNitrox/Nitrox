using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Setups the local player when it stops being locked (e.g. when using the freecam command)
/// </summary>
public sealed partial class Player_ExitLockedMode_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.ExitLockedMode(default, default));

    public static void Postfix(Player __instance) => Player_ExitPilotingMode_Patch.Postfix(__instance);
}
