using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Event when someone picks up an item.
/// </summary>
public sealed partial class ResourceTracker_OnPickedUp_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ResourceTracker d) => d.OnPickedUp(default));

    public static bool Prefix(ResourceTracker __instance)
    {
        TechType instanceTechType = __instance.techType;
        GameObject instanceGameObject = __instance.gameObject;
        if (instanceTechType != null && instanceGameObject != null)
        {
            Resolve<Items>().PickedUp(instanceGameObject, instanceTechType);
        }
        return true;
    }
}
