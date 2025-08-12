using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="CreatureAction.StopPerform"/> from happening if local player doesn't have lock on creature or if the action is not whitelisted
/// </summary>
public sealed partial class CreatureAction_StopPerform_Patch : NitroxPatch, IDynamicPatch
{
#if SUBNAUTICA
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureAction t) => t.StopPerform(default, default));
#elif BELOWZERO
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureAction t) => t.StopPerform(default));
#endif

    public static bool Prefix(CreatureAction __instance) => CreatureAction_StartPerform_Patch.Prefix(__instance);
}
