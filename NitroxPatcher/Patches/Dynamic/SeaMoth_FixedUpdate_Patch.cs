using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables <see cref="SeaMoth.FixedUpdate"/> for not simulated Seamoths.
/// </summary>
public sealed partial class Seamoth_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((SeaMoth t) => t.FixedUpdate());

    public static bool Prefix(SeaMoth __instance)
    {
        return !__instance.GetComponent<MovementReplicator>();
    }
}
