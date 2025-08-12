using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// For players without lock: apply remote creature actions and prevent the original call.
/// For players with lock: broadcast new creature actions
/// </summary>
public sealed partial class Creature_ChooseBestAction_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Creature t) => t.ChooseBestAction(default));

    public static bool Prefix(Creature __instance, out NitroxId __state, ref CreatureAction __result)
    {
        if (!__instance.TryGetIdOrWarn(out __state) || Resolve<SimulationOwnership>().HasAnyLockType(__state))
        {
            return true;
        }

        // If we have received any order
        if (Resolve<AI>().TryGetActionForCreature(__instance, out CreatureAction action))
        {
            __result = action;
        }
        return false;
    }

    public static void Postfix(Creature __instance, bool __runOriginal, NitroxId __state, ref CreatureAction __result)
    {
        if (!__runOriginal || __state == null)
        {
            return;
        }

        if (Resolve<SimulationOwnership>().HasAnyLockType(__state))
        {
            Resolve<AI>().BroadcastNewAction(__state, __instance, __result);
        }
    }
}
