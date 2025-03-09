using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Drillable_OnDrill_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Drillable t) => t.OnDrill(default, default, out Reflect.Ref<GameObject>.Field));

    public static void Postfix(Drillable __instance)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance, id);
        }
    }
}
