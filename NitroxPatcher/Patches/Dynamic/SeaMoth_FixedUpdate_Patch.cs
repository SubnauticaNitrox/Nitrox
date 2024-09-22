using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Seamoth_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaMoth t) => t.FixedUpdate());

    public static bool Prefix(SeaMoth __instance)
    {
        return !__instance.GetComponent<MovementReplicator>();
    }
}
