using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables <see cref="Exosuit.FixedUpdate"/> for not simulated Exosuits.
/// </summary>
public sealed partial class Exosuit_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((Exosuit t) => t.FixedUpdate());

    public static bool Prefix(Exosuit __instance)
    {
        return !__instance.GetComponent<MovementReplicator>();
    }
}
