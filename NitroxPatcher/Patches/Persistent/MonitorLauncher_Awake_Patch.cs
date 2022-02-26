using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public class MonitorLauncher_Awake_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((MonitorLauncher t) => t.Awake());

    public static void Postfix(MonitorLauncher __instance)
    {
        UnityEngine.Object.Destroy(__instance);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, targetMethod);
    }
}
