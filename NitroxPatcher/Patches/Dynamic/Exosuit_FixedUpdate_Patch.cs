using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Exosuit_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Exosuit t) => t.FixedUpdate());

    public static bool Prefix(Exosuit __instance)
    {
        return !__instance.GetComponent<MovementReplicator>();
    }
}
