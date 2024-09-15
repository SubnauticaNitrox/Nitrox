using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using static NitroxModel.Helper.Reflect;

namespace NitroxPatcher.Patches.Dynamic;

public sealed class CyclopsDestructionEvent_OnConsoleCommand_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD_RESTORE = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_restorecyclops(default));
    private static readonly MethodInfo TARGET_METHOD_DESTROY = Reflect.Method((CyclopsDestructionEvent t) => t.OnConsoleCommand_destroycyclops(default));

    public static bool PrefixRestore()
    {
        Log.InGame(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }

    public static bool PrefixDestroy(CyclopsDestructionEvent __instance, out bool __state)
    {
        // We only apply the destroy to the current Cyclops
        __state = Player.main.currentSub == __instance.subRoot;
        return __state;
    }

    /// <remarks>
    /// This must happen at postfix so that the SubRootChanged packet are sent before the destroyed vehicle one,
    /// thus saving player entities from deletion.
    /// </remarks>
    public static void PostfixDestroy(CyclopsDestructionEvent __instance, bool __state)
    {
        if (__state && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Vehicles>().BroadcastDestroyedVehicle(id);
        }
    }

    public override void Patch(Harmony harmony)
    {
        MethodInfo destroyPrefixInfo = Method(() => PrefixDestroy(default, out Ref<bool>.Field));

        PatchPrefix(harmony, TARGET_METHOD_RESTORE, ((Func<bool>)PrefixRestore).Method);
        PatchPrefix(harmony, TARGET_METHOD_DESTROY, destroyPrefixInfo);
        PatchPostfix(harmony, TARGET_METHOD_DESTROY, ((Action<CyclopsDestructionEvent, bool>)PostfixDestroy).Method);
    }
}
