using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents remote players from being frozen by remote stasis fields
/// </summary>
public sealed partial class StasisSphere_Freeze_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StasisSphere t) => t.Freeze(default, ref Reflect.Ref<Rigidbody>.Field));

    public static bool Prefix(Collider other, ref bool __result)
    {
        return !other.GetComponentInParent<RemotePlayerIdentifier>(true);
    }
}
