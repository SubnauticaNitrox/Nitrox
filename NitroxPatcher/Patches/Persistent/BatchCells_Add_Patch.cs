using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    public class BatchCells_Add_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BatchCells);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Add");
        
        public static void Postfix(BatchCells __instance, EntityCell __result)
        {
            EntityCellCleaner.Main.Add(__result);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
        
    }
}
