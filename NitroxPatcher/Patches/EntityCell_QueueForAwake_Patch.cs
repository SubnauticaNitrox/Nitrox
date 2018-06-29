using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    public class EntityCell_QueueForAwake_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EntityCell);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("QueueForAwake");

        public static bool Prefix(EntityCell __instance)
        {
            //NitroxServiceLocator.LocateService<Terrain>().CellLoaded(__instance.BatchId, __instance.CellId, __instance.Level);

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
