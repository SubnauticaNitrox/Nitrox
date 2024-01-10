using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="CreatureAction.Perform"/> from happening if local player doesn't have lock on creature
/// </summary>
public sealed partial class CreatureAction_Perform_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureAction t) => t.Perform(default, default, default));

    public static bool Prefix(CreatureAction __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId id) || Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            return true;
        }

        // Perform should never be done on non-executing clients because it usually only calls swim behaviours
        return false;
    }
}
