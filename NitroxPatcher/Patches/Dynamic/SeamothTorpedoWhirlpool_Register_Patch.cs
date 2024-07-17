using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents remote players from being drawn by torpedo's whirlpools
/// </summary>
public sealed partial class SeamothTorpedoWhirlpool_Register_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeamothTorpedoWhirlpool t) => t.Register(default, ref Reflect.Ref<Rigidbody>.Field));

    public static bool Prefix(Collider other)
    {
        return !other.GetComponentInParent<RemotePlayerIdentifier>(true);
    }
}
