using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

internal class BaseDeconstructable_DeconstructionAllowed_Patch : NitroxPatch, IDynamicPatch
{
    internal static MethodInfo TARGET_METHOD = Reflect.Method((BaseDeconstructable t) => t.DeconstructionAllowed(out Reflect.Ref<string>.Field));

    public static void Postfix(BaseDeconstructable __instance, ref bool __result, ref string reason)
    {
        if (!__result || !BuildingHandler.Main || !__instance.deconstructedBase.TryGetComponent(out NitroxEntity parentEntity))
        {
            return;
        }
        Constructable_DeconstructionAllowed_Patch.DeconstructionAllowed(parentEntity.Id, ref __result, ref reason);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
