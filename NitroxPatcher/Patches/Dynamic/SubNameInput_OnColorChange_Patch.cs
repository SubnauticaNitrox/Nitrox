using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SubNameInput_OnColorChange_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubNameInput t) => t.OnColorChange(default(ColorChangeEventData)));

    public static void Postfix(SubNameInput __instance)
    {
        if (SubNameInput_OnNameChange_Patch.TryGetTargetId(__instance, out object target, out NitroxId targetId))
        {
            Resolve<Entities>().EntityMetadataChangedThrottled(target, targetId, 0.1f);
        }
    }
}
