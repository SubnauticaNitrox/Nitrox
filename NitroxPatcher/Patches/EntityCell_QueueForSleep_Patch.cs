using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    /*//#gitignore
    public class EntityCell_QueueForSleep_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EntityCell);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QueueForSleep");

        public static bool Prefix(EntityCell __instance)
        {
            NitroxServiceLocator.LocateService<Terrain>().CellUnloaded(__instance.BatchId, __instance.CellId, __instance.Level);
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
    *///#gitignore
}
