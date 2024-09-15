using System.Reflection;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Ensures every spawned Cyclops (except for virtual ones) has a <see cref="NitroxCyclops"/> script attached to it.
/// </summary>
public sealed partial class SubControl_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubControl t) => t.Start());

    public static void Postfix(SubControl __instance)
    {
        if (__instance.name != VirtualCyclops.NAME && __instance.name != LightmappedPrefabs.StandardMainObjectName)
        {
            __instance.gameObject.EnsureComponent<NitroxCyclops>();
        }
    }
}
