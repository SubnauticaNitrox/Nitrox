using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <inheritdoc cref="PickPrefab_AddToContainerAsync_Patch"/>
public sealed partial class PickPrefab_SetPickedState_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PickPrefab t) => t.SetPickedState(default));

    public static void Postfix(PickPrefab __instance, bool newPickedState)
    {
        if (newPickedState && __instance.destroyOnPicked)
        {
            PickPrefab_AddToContainerAsync_Patch.BroadcastDeletion(__instance);
        }
    }
}
