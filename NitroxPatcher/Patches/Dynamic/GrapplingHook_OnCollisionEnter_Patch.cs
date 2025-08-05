using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevent grappling hooks fired from other players colliding with the local player
/// (they do not collide with the remote player on the piloting player's end for some reason)
/// </summary>
public sealed partial class GrapplingHook_OnCollisionEnter_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GrapplingHook t) => t.OnCollisionEnter(default));

    public static bool Prefix(Collision collisionInfo)
    {
        return !collisionInfo.gameObject.GetComponent<Player>();
    }
}
