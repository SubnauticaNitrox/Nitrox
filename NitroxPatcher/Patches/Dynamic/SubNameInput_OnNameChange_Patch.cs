using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SubNameInput_OnNameChange_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubNameInput t) => t.OnNameChange(default(string)));

    public static void Postfix(SubNameInput __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId subNameInputId))
        {
            // prevent this patch from firing when the initial template cyclops loads (happens on game load with living large update).
            return;
        }

        SubName subName = __instance.target;
        if (subName)
        {
            if (subName.TryGetComponentInParent(out Rocket rocket, true))
            {
                // For some reason only the rocket has a full functioning ghost with a different NitroxId when spawning/constructing, so we are ignoring it.
                if (rocket.TryGetComponentInChildren(out VFXConstructing constructing, true) && !constructing.isDone)
                {
                    return;
                }
            }
            else if (!subName.TryGetComponent(out Vehicle _) && !subName.TryGetComponentInParent(out SubRoot _, true))
            {
                Log.Error($"[SubNameInput_OnNameChange_Patch] The GameObject {subName.gameObject.name} doesn't have a Vehicle/SubRoot/Rocket component.");
                return;
            }

            Resolve<Entities>().EntityMetadataChanged(__instance, subNameInputId);
        }
    }
}
