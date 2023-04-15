using System.Reflection;
using HarmonyLib;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public class SubNameInput_OnColorChange_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubNameInput t) => t.OnColorChange(default(ColorChangeEventData)));

    public static void Postfix(SubNameInput __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId subNameId))
        {
            // prevent this patch from firing when the initial template cyclops loads (happens on game load with living large update).
            return;
        }

        SubName subName = __instance.target;
        if (!subName)
        {
            return;
        }

        if (subName.TryGetComponentInParent(out Rocket rocket))
        {
            // For some reason only the rocket has a full functioning ghost with a different NitroxId when spawning/constructing, so we are ignoring it.
            if (rocket.TryGetComponentInChildren(out VFXConstructing constructing) && !constructing.isDone)
            {
                return;
            }
        }
        else if (!subName.TryGetComponent(out Vehicle _) && !subName.TryGetComponentInParent(out SubRoot _))
        {
            Log.Error($"[SubNameInput_OnColorChange_Patch] The GameObject {subName.gameObject.name} doesn't have a Vehicle/SubRoot/Rocket component.");
            return;
        }

        Resolve<Entities>().EntityMetadataChangedThrottled(__instance, subNameId);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
