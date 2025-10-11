using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents deconstruction if the target base is desynced.
/// </summary>
public sealed partial class Constructable_DeconstructionAllowed_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.DeconstructionAllowed(out Reflect.Ref<string>.Field));

    public static void Postfix(Constructable __instance, ref bool __result, ref string reason)
    {
        if (!__result || !BuildingHandler.Main || !__instance.TryGetComponentInParent(out NitroxEntity parentEntity, true))
        {
            return;
        }
        BuildUtils.DeconstructionAllowed(parentEntity.Id, ref __result, ref reason);
    }
}
