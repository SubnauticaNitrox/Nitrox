using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="CreatureAction.StartPerform"/> from happening if local player doesn't have lock on creature or if the action is not whitelisted
/// </summary>
public sealed partial class CreatureAction_StartPerform_Patch : NitroxPatch, IDynamicPatch
{
#if SUBNAUTICA
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureAction t) => t.StartPerform(default, default));
#elif BELOWZERO
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureAction t) => t.StartPerform(default));
#endif

    public static bool Prefix(CreatureAction __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId id) || Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            return true;
        }

        return Resolve<AI>().IsCreatureActionWhitelisted(__instance);
    }
}
