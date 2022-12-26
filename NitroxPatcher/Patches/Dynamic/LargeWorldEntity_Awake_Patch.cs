using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// We don't want constructables to be put in CellRoots but in GlobalRoot, so we change a parameter in their LargeWorldEntity.
/// </summary>
public class LargeWorldEntity_Awake_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LargeWorldEntity t) => t.Awake());

    public static void Prefix(LargeWorldEntity __instance)
    {
        if (__instance.cellLevel < LargeWorldEntity.CellLevel.Global &&
            __instance.GetComponent<Constructable>())
        {
            __instance.cellLevel = LargeWorldEntity.CellLevel.Global;
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
