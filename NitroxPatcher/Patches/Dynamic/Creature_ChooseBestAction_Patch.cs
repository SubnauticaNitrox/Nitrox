using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Creature_ChooseBestAction_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Creature t) => t.ChooseBestAction(default(float)));

    private static CreatureAction previousAction;

    public static bool Prefix(Creature __instance, ref CreatureAction __result)
    {
        if (!__instance.TryGetNitroxId(out NitroxId id))
        {
            Log.WarnOnce($"[Creature_ChooseBestAction_Patch] Couldn't find an id on {__instance.GetFullHierarchyPath()}");
            return true;
        }

        if (Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            previousAction = __instance.prevBestAction;
            return true;
        }

        // CreatureActionChangedProcessor.ActionById.TryGetValue(id, out __result);

        return false;
    }

    // TODO: Postfix disabled for the moment as it has no functionality
    public static void Postfix_disabled(Creature __instance, ref CreatureAction __result)
    {
        if (!__instance.TryGetIdOrWarn(out NitroxId id))
        {
            return;
        }

        if (Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            if (previousAction != __result)
            {
                // Multiplayer.Logic.AI.CreatureActionChanged(id, __result);
            }
        }
    }
}
