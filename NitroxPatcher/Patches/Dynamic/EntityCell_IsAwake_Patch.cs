using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents a cell from being considered awake if its contents weren't spawned
/// </summary>
public sealed partial class EntityCell_IsAwake_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EntityCell t) => t.IsAwake());

    public static void Postfix(EntityCell __instance, ref bool __result)
    {
        if (__result)
        {
            __result = Resolve<Terrain>().IsCellFullySpawned(__instance.batchId, __instance.cellId, __instance.level);
        }
    }
}
