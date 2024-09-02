using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Allow for openables to detect collisions with pawns in the virtual cyclops.
/// </summary>
public sealed partial class Openable_OnTriggerExit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Openable t) => t.OnTriggerExit(default));

    public static void Prefix(Openable __instance, Collider collider)
    {
        if (collider.TryGetComponent(out CyclopsPawnIdentifier identifier))
        {
            identifier.Pawn.BlockOpenable(__instance, false);
        }
    }
}
