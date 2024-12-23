using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// An important source of cell unload which is different than <see cref="EntityCell.SleepAsync"/> but must also be taken into account.
/// </summary>
public sealed partial class EntityCell_Reset_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((EntityCell t) => t.Reset());

    public static void Prefix(EntityCell __instance)
    {
        Resolve<Terrain>().CellUnloaded(__instance.batchId, __instance.cellId, __instance.level);
    }
}
