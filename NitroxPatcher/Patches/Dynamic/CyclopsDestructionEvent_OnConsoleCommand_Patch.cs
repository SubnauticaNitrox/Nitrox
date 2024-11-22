using System;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using static NitroxModel.Helper.Reflect;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class CyclopsDestructionEvent_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD_RESTORE = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_restorecyclops(default));
    private static readonly MethodInfo TARGET_METHOD_DESTROY = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_destroycyclops(default));

    public static bool PrefixRestore()
    {
        // TODO: add support for "restorecyclops" command
        Log.InGame(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }

    public static bool PrefixDestroy(CyclopsDestructionEvent __instance, out bool __state)
    {
        // We only apply the destroy to the current Cyclops
        __state = Player.main.currentSub == __instance.subRoot;
        return __state;
    }

    public override void Patch(Harmony harmony)
    {
        MethodInfo destroyPrefixInfo = Method(() => PrefixDestroy(default, out Ref<bool>.Field));

        PatchPrefix(harmony, TARGET_METHOD_RESTORE, ((Func<bool>)PrefixRestore).Method);
        PatchPrefix(harmony, TARGET_METHOD_DESTROY, destroyPrefixInfo);
    }
}
