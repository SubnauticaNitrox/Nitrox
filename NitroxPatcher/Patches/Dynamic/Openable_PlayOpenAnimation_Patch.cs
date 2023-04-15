using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class Openable_PlayOpenAnimation_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Openable t) => t.PlayOpenAnimation(default(bool), default(float)));

    public static bool Prefix(Openable __instance, bool openState, float duration)
    {
        if (__instance.isOpen != openState && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Interior>().OpenableStateChanged(id, openState, duration);
        }

        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
